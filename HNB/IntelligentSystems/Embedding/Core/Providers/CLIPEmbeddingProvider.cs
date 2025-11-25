using HNB.IntelligentSystems.Embedding.Configuration;
using Microsoft.ML.OnnxRuntime.Tensors;
using OnnxSessionOptions = Microsoft.ML.OnnxRuntime.SessionOptions;
using OnnxInferenceSession = Microsoft.ML.OnnxRuntime.InferenceSession;
using OnnxGraphOptimizationLevel = Microsoft.ML.OnnxRuntime.GraphOptimizationLevel;

namespace HNB.IntelligentSystems.Embedding.Core.Providers;

/// <summary>
/// CLIP 嵌入提供者
/// </summary>
public class CLIPEmbeddingProvider : IEmbeddingProvider, IDisposable
{
    private readonly CLIPModelConfig _config;
    private readonly IWebHostEnvironment _environment;
    private OnnxInferenceSession? _visionEncoderSession;
    private OnnxInferenceSession? _textEncoderSession;
    private OnnxInferenceSession? _visionProjectionSession;
    private OnnxInferenceSession? _textProjectionSession;
    private CLIPTextTokenizer? _tokenizer;
    private readonly object _lockObject = new();

    public string Name => _config.Name;
    public int VectorSize => _config.VectorSize;
    public bool SupportsText => true;
    public bool SupportsImage => true;

    public CLIPEmbeddingProvider(CLIPModelConfig config, IWebHostEnvironment environment)
    {
        _config = config;
        _environment = environment;
    }

    private bool EnsureModelsLoaded()
    {
        if (_visionEncoderSession != null && _textEncoderSession != null &&
            _visionProjectionSession != null && _textProjectionSession != null)
            return true;

        lock (_lockObject)
        {
            if (_visionEncoderSession != null && _textEncoderSession != null &&
                _visionProjectionSession != null && _textProjectionSession != null)
                return true;

            var modelPath = Path.Combine(_environment.ContentRootPath, _config.ModelPath);
            var sessionOptions = new OnnxSessionOptions();
            sessionOptions.GraphOptimizationLevel = OnnxGraphOptimizationLevel.ORT_ENABLE_BASIC;

            var visionEncoderPath = Path.Combine(modelPath, "clip_vision_encoder.onnx");
            var textEncoderPath = Path.Combine(modelPath, "clip_text_encoder.onnx");
            var visionProjectionPath = Path.Combine(modelPath, "clip_vision_projection.onnx");
            var textProjectionPath = Path.Combine(modelPath, "clip_text_projection.onnx");

            if (!File.Exists(visionEncoderPath) || !File.Exists(textEncoderPath) ||
                !File.Exists(visionProjectionPath) || !File.Exists(textProjectionPath))
                return false;

            try
            {
                _visionEncoderSession = new OnnxInferenceSession(visionEncoderPath, sessionOptions);
                _textEncoderSession = new OnnxInferenceSession(textEncoderPath, sessionOptions);
                _visionProjectionSession = new OnnxInferenceSession(visionProjectionPath, sessionOptions);
                _textProjectionSession = new OnnxInferenceSession(textProjectionPath, sessionOptions);

                var vocabPath = Path.Combine(modelPath, "vocab.json");
                var mergesPath = Path.Combine(modelPath, "merges.txt");

                if (File.Exists(vocabPath) && File.Exists(mergesPath))
                {
                    _tokenizer = new CLIPTextTokenizer();
                    if (!_tokenizer.LoadTokenizer(vocabPath, mergesPath))
                        _tokenizer = null;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public async Task<List<float>> EncodeTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text) || !EnsureModelsLoaded() ||
            _textEncoderSession == null || _textProjectionSession == null || _tokenizer == null)
            return new List<float>();

        var (tokenIds, actualLength) = _tokenizer.EncodeWithLength(text);
        var inputTensor = new DenseTensor<long>(tokenIds, new[] { 1, tokenIds.Length });

        var attentionMask = new long[tokenIds.Length];
        for (int i = 0; i < tokenIds.Length; i++)
            attentionMask[i] = i < actualLength ? 1L : 0L;
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length });

        var encoderInputs = new List<Microsoft.ML.OnnxRuntime.NamedOnnxValue>
        {
            Microsoft.ML.OnnxRuntime.NamedOnnxValue.CreateFromTensor("input_ids", inputTensor),
            Microsoft.ML.OnnxRuntime.NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor)
        };

        using var encoderResults = _textEncoderSession.Run(encoderInputs);
        var encoderOutput = encoderResults.First().Value as DenseTensor<float>;
        if (encoderOutput == null)
            return new List<float>();

        var encoderDims = encoderOutput.Dimensions.ToArray();
        DenseTensor<float> pooledOutput;

        if (encoderDims.Length == 3)
        {
            var batchSize = encoderDims[0];
            var seqLen = encoderDims[1];
            var hiddenSize = encoderDims[2];
            int lastValidTokenIndex = Math.Max(0, Math.Min(actualLength - 1, seqLen - 1));

            pooledOutput = new DenseTensor<float>(new[] { batchSize, hiddenSize });
            for (int i = 0; i < hiddenSize; i++)
                pooledOutput[0, i] = encoderOutput[0, lastValidTokenIndex, i];
        }
        else if (encoderDims.Length == 2)
        {
            pooledOutput = encoderOutput;
        }
        else
        {
            return new List<float>();
        }

        var projectionInputMetadata = _textProjectionSession.InputMetadata;
        if (projectionInputMetadata.Count == 0)
            return new List<float>();

        var projectionInputName = projectionInputMetadata.Keys.First();
        var projectionInputs = new List<Microsoft.ML.OnnxRuntime.NamedOnnxValue>
        {
            Microsoft.ML.OnnxRuntime.NamedOnnxValue.CreateFromTensor(projectionInputName, pooledOutput)
        };

        using var projectionResults = _textProjectionSession.Run(projectionInputs);
        var projectionOutput = projectionResults.First().Value as DenseTensor<float>;
        if (projectionOutput == null)
            return new List<float>();

        var vector = new List<float>();
        var dims = projectionOutput.Dimensions.ToArray();

        if (dims.Length == 2)
        {
            for (int i = 0; i < dims[1]; i++)
                vector.Add(projectionOutput[0, i]);
        }
        else if (dims.Length == 3)
        {
            var seqLen = dims[1];
            var hiddenSize = dims[2];
            for (int i = 0; i < hiddenSize; i++)
                vector.Add(projectionOutput[0, seqLen - 1, i]);
        }

        await Task.CompletedTask;
        return vector;
    }

    public async Task<List<float>> EncodeImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return new List<float>();
    }

    public void Dispose()
    {
        _visionEncoderSession?.Dispose();
        _textEncoderSession?.Dispose();
        _visionProjectionSession?.Dispose();
        _textProjectionSession?.Dispose();
    }
}

