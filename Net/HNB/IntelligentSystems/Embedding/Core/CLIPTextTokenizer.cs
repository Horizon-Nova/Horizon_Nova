using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HNB.IntelligentSystems.Embedding.Core;

/// <summary>
/// CLIP 文本分詞器
/// </summary>
public class CLIPTextTokenizer
{
    private Dictionary<string, int> _vocab = new();
    private Dictionary<(string, string), int> _bpeRanks = new();
    private const string StartOfText = "<|startoftext|>";
    private const string EndOfText = "<|endoftext|>";
    private const int MaxTextLength = 77; // CLIP 的標準文本長度

    /// <summary>
    /// 載入配置
    /// </summary>
    public bool LoadTokenizer(string vocabPath, string mergesPath)
    {
        if (!File.Exists(vocabPath) || !File.Exists(mergesPath))
            return false;

        // 載入 vocab.json
        var vocabJson = File.ReadAllText(vocabPath, Encoding.UTF8);
        var vocabDict = JsonSerializer.Deserialize<Dictionary<string, int>>(vocabJson);
        if (vocabDict == null)
            return false;

        _vocab = vocabDict;

        // 載入 merges.txt
        var mergesLines = File.ReadAllLines(mergesPath, Encoding.UTF8);
        _bpeRanks.Clear();

        int rank = 0;
        foreach (var line in mergesLines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                continue;

            var parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                _bpeRanks[(parts[0], parts[1])] = rank++;
            }
        }

        return _vocab.Count > 0 && _bpeRanks.Count > 0;
    }

    /// <summary>
    /// 編碼文本
    /// </summary>
    public long[] Encode(string text)
    {
        var (tokenIds, _) = EncodeWithLength(text);
        return tokenIds;
    }

    /// <summary>
    /// 編碼文本並返回長度
    /// </summary>
    public (long[] tokenIds, int actualLength) EncodeWithLength(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            text = "";

        // 添加特殊 token
        text = $"{StartOfText} {text} {EndOfText}";

        // 將文本轉換為 BPE tokens
        var bpeTokens = BpeEncode(text);

        // 將 BPE tokens 轉換為 token IDs
        var tokenIds = new List<long>();
        foreach (var token in bpeTokens)
        {
            if (_vocab.TryGetValue(token, out var id))
                tokenIds.Add(id);
            else
                tokenIds.Add(_vocab.GetValueOrDefault(EndOfText, 0));
        }

        var actualLength = tokenIds.Count;

        // 截斷或填充到固定長度
        var result = new long[MaxTextLength];
        var endOfTextId = _vocab.GetValueOrDefault(EndOfText, 0);
        
        for (int i = 0; i < MaxTextLength; i++)
        {
            if (i < tokenIds.Count)
                result[i] = tokenIds[i];
            else
                result[i] = endOfTextId;
        }

        return (result, Math.Min(actualLength, MaxTextLength));
    }

    private List<string> BpeEncode(string text)
    {
        // 將文本轉換為 bytes，然後轉換為 tokens
        var bytes = Encoding.UTF8.GetBytes(text);
        var tokens = new List<string>();
        
        foreach (var b in bytes)
        {
            var token = $"<0x{b:X2}>";
            tokens.Add(token);
        }

        // 應用 BPE 合併規則
        while (true)
        {
            var pairs = GetPairs(tokens);
            if (pairs.Count == 0)
                break;

            var bigram = GetMinRankPair(pairs);
            if (bigram == null || !_bpeRanks.ContainsKey(bigram.Value))
                break;

            var first = bigram.Value.Item1;
            var second = bigram.Value.Item2;
            var newTokens = new List<string>();
            var i = 0;

            while (i < tokens.Count)
            {
                var j = FindIndex(tokens, first, i);
                if (j != -1)
                {
                    newTokens.AddRange(tokens.GetRange(i, j - i));
                    i = j;
                }
                else
                {
                    newTokens.AddRange(tokens.GetRange(i, tokens.Count - i));
                    break;
                }

                if (i < tokens.Count - 1 && tokens[i] == first && tokens[i + 1] == second)
                {
                    newTokens.Add(first + second);
                    i += 2;
                }
                else
                {
                    newTokens.Add(tokens[i]);
                    i += 1;
                }
            }

            tokens = newTokens;
        }

        return tokens;
    }

    private HashSet<(string, string)> GetPairs(List<string> tokens)
    {
        var pairs = new HashSet<(string, string)>();
        if (tokens.Count < 2)
            return pairs;

        for (int i = 0; i < tokens.Count - 1; i++)
        {
            pairs.Add((tokens[i], tokens[i + 1]));
        }

        return pairs;
    }

    private (string, string)? GetMinRankPair(HashSet<(string, string)> pairs)
    {
        (string, string)? minPair = null;
        int minRank = int.MaxValue;

        foreach (var pair in pairs)
        {
            if (_bpeRanks.TryGetValue(pair, out var rank) && rank < minRank)
            {
                minRank = rank;
                minPair = pair;
            }
        }

        return minPair;
    }

    private int FindIndex(List<string> list, string item, int startIndex)
    {
        for (int i = startIndex; i < list.Count; i++)
        {
            if (list[i] == item)
                return i;
        }
        return -1;
    }
}

