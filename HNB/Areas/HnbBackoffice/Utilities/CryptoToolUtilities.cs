// File: CryptoToolUtilities.cs
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace HNB.Areas.HnbBackoffice.Utilities
{
    /// <summary> 單向加密工具：SHA-256(plaintext) → Argon2id(…, salt) </summary>
    public static class CryptoToolUtilities
    {
        // ---- 安全參數固定 ----
        private const int SALT_BYTES = 16;     // 建議 16/32 任一，固定即可
        private const int ARGON_MEMORY_KB = 65536;  // 64 MiB
        private const int ARGON_ITERATIONS = 3;
        private const int ARGON_PARALLELISM = 2;
        private const int OUTPUT_BYTES = 32;     // 256-bit

        /// <summary>
        /// 產生雜湊（自動隨機 salt）。
        /// </summary>
        public static KdfResult HashSha256ThenArgon2id(string plaintext)
        {
            var salt = RandomBytes(SALT_BYTES);
            return Compute(plaintext, salt);
        }

        /// <summary>
        /// 產生雜湊（使用外部提供的 saltBase64；用於驗證或模擬真實情境）。
        /// </summary>
        public static KdfResult HashSha256ThenArgon2id(string plaintext, string saltBase64)
        {
            if (string.IsNullOrWhiteSpace(saltBase64))
                throw new ArgumentException("saltBase64 不可為空。", nameof(saltBase64));

            byte[] salt;
            try { salt = Convert.FromBase64String(saltBase64); }
            catch { throw new ArgumentException("saltBase64 格式錯誤（不是有效的 Base64）。", nameof(saltBase64)); }

            if (salt.Length < 16)
                throw new ArgumentException("salt 長度至少 16 bytes。", nameof(saltBase64));

            return Compute(plaintext, salt);
        }

        // ---- Core ----
        private static KdfResult Compute(string plaintext, byte[] salt)
        {
            if (plaintext is null) plaintext = string.Empty;

            // SHA-256 預雜湊
            byte[] prehash;
            using (var sha = SHA256.Create())
                prehash = sha.ComputeHash(Encoding.UTF8.GetBytes(plaintext));

            // Argon2id
            var argon = new Argon2id(prehash)
            {
                Salt = salt,
                DegreeOfParallelism = ARGON_PARALLELISM,
                MemorySize = ARGON_MEMORY_KB,
                Iterations = ARGON_ITERATIONS
            };
            var hash = argon.GetBytes(OUTPUT_BYTES);

            var saltB64 = Convert.ToBase64String(salt);
            var hashB64 = Convert.ToBase64String(hash);
            var phc = $"$argon2id$v=19$m={ARGON_MEMORY_KB},t={ARGON_ITERATIONS},p={ARGON_PARALLELISM}${saltB64}${hashB64}";

            return new KdfResult { SaltBase64 = saltB64, HashBase64 = hashB64, Phc = phc };
        }

        // ---- helpers ----
        private static byte[] RandomBytes(int len)
        {
            var b = new byte[len];
            RandomNumberGenerator.Fill(b);
            return b;
        }
    }

    public sealed class KdfResult
    {
        public string SaltBase64 { get; set; } = string.Empty;
        public string HashBase64 { get; set; } = string.Empty;
        public string Phc { get; set; } = string.Empty;
    }
}
