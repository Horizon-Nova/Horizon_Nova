using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HNB.IntelligentSystems.ObjectDetection.Core
{
    /// <summary>
    /// BERT-based text tokenizer for converting text prompts into model inputs.
    /// </summary>
    public class TextTokenizer
    {
        private Dictionary<string, int> vocab = new Dictionary<string, int>();
        private Dictionary<int, string> invVocab = new Dictionary<int, string>();
        private const int DefaultMaxTextLen = 256;
        private readonly string[] specialTexts = { "[CLS]", "[SEP]", ".", "?" };

        /// <summary>
        /// Loads the vocabulary from a file.
        /// </summary>
        /// <param name="vocabPath">Path to the vocabulary file.</param>
        /// <returns>True if loaded successfully, false otherwise.</returns>
        public bool LoadVocab(string vocabPath)
        {
            if (!File.Exists(vocabPath))
                return false;

            vocab.Clear();
            invVocab.Clear();

            int index = 0;
            foreach (string line in File.ReadLines(vocabPath, Encoding.UTF8))
            {
                string token = line.Trim();
                if (string.IsNullOrEmpty(token))
                    continue;

                vocab[token] = index;
                invVocab[index] = token;
                index++;
            }

            return vocab.Count > 0;
        }

        /// <summary>
        /// Tokenizes a text string into tokens.
        /// </summary>
        private List<string> Tokenize(string text)
        {
            text = text.ToLower().Trim();

            List<string> tokens = new List<string>();
            StringBuilder currentToken = new StringBuilder();

            foreach (char c in text)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.AddRange(TokenizeWord(currentToken.ToString()));
                        currentToken.Clear();
                    }
                }
                else if (IsPunctuation(c))
                {
                    if (currentToken.Length > 0)
                    {
                        tokens.AddRange(TokenizeWord(currentToken.ToString()));
                        currentToken.Clear();
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    currentToken.Append(c);
                }
            }

            if (currentToken.Length > 0)
            {
                tokens.AddRange(TokenizeWord(currentToken.ToString()));
            }

            return tokens;
        }

        private List<string> TokenizeWord(string word)
        {
            List<string> wordPieces = new List<string>();

            int start = 0;
            while (start < word.Length)
            {
                int end = word.Length;
                string? subWord = null;

                while (start < end)
                {
                    string candidate = start > 0 
                        ? "##" + word.Substring(start, end - start) 
                        : word.Substring(start, end - start);
                    
                    if (vocab.ContainsKey(candidate))
                    {
                        subWord = candidate;
                        break;
                    }
                    end--;
                }

                if (subWord == null)
                {
                    wordPieces.Add("[UNK]");
                    break;
                }

                wordPieces.Add(subWord);
                start = end;
            }

            return wordPieces;
        }

        private bool IsPunctuation(char c)
        {
            int cp = (int)c;
            return ((cp >= 33 && cp <= 47) || (cp >= 58 && cp <= 64) ||
                    (cp >= 91 && cp <= 96) || (cp >= 123 && cp <= 126)) ||
                   char.IsPunctuation(c);
        }

        /// <summary>
        /// Converts tokens to token IDs.
        /// </summary>
        public long[] ConvertTokensToIds(List<string> tokens)
        {
            long[] ids = new long[tokens.Count];
            for (int i = 0; i < tokens.Count; i++)
            {
                ids[i] = vocab.ContainsKey(tokens[i]) ? vocab[tokens[i]] : vocab.GetValueOrDefault("[UNK]", 0);
            }
            return ids;
        }

        /// <summary>
        /// Converts a token ID back to its token string.
        /// </summary>
        public string ConvertIdToToken(long tokenId)
        {
            return invVocab.GetValueOrDefault((int)tokenId, "[UNK]");
        }

        /// <summary>
        /// Tokenizes text into input IDs, token type IDs, attention mask, and special tokens.
        /// </summary>
        public (long[] inputIds, long[] tokenTypeIds, bool[] attentionMask, List<int> specialTokens) TokenizeText(
            string text, int contextLength = 256)
        {
            text = text.Trim();
            if (!text.EndsWith("."))
                text += " .";

            List<string> tokens = Tokenize(text);
            int maxTokens = contextLength - 2;
            if (tokens.Count > maxTokens)
                tokens = tokens.Take(maxTokens).ToList();

            long clsId = vocab["[CLS]"];
            List<long> inputIds = new List<long> { clsId };

            long[] tokenIds = ConvertTokensToIds(tokens);
            inputIds.AddRange(tokenIds);

            long sepId = vocab["[SEP]"];
            inputIds.Add(sepId);

            int seqLen = inputIds.Count;
            long[] tokenTypeIds = new long[seqLen];
            bool[] attentionMask = new bool[seqLen];
            for (int i = 0; i < seqLen; i++)
                attentionMask[i] = true;

            List<int> specialTokens = new List<int>();
            foreach (string specialText in specialTexts)
            {
                if (vocab.ContainsKey(specialText))
                    specialTokens.Add(vocab[specialText]);
            }

            return (inputIds.ToArray(), tokenTypeIds, attentionMask, specialTokens);
        }

        /// <summary>
        /// Generates attention masks and position IDs with special tokens.
        /// </summary>
        public (bool[,,] textSelfAttentionMasks, long[] positionIds) GenerateMasksWithSpecialTokens(
            long[] inputIds, List<int> specialTokens)
        {
            int batchSize = 1;
            int numToken = inputIds.Length;

            bool[] specialTokensMask = new bool[numToken];
            foreach (int specialToken in specialTokens)
            {
                for (int i = 0; i < numToken; i++)
                {
                    if (inputIds[i] == specialToken)
                        specialTokensMask[i] = true;
                }
            }

            List<(int row, int col)> idxs = new List<(int, int)>();
            for (int i = 0; i < numToken; i++)
            {
                if (specialTokensMask[i])
                    idxs.Add((0, i));
            }

            bool[,,] textSelfAttentionMasks = new bool[batchSize, numToken, numToken];
            for (int i = 0; i < numToken; i++)
            {
                textSelfAttentionMasks[0, i, i] = true;
            }

            long[] positionIds = new long[numToken];

            int previousCol = 0;
            foreach (var (row, col) in idxs)
            {
                if (col == 0 || col == numToken - 1)
                {
                    textSelfAttentionMasks[row, col, col] = true;
                    positionIds[col] = 0;
                }
                else
                {
                    for (int i = previousCol + 1; i <= col; i++)
                    {
                        for (int j = previousCol + 1; j <= col; j++)
                        {
                            textSelfAttentionMasks[row, i, j] = true;
                        }
                        positionIds[i] = i - (previousCol + 1);
                    }
                }
                previousCol = col;
            }

            return (textSelfAttentionMasks, positionIds);
        }
    }
}

