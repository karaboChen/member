using System.Security.Cryptography;
using System.Text;

namespace member.Utils
{
    public static class EncryptionHelper
    {
        /// <summary>
        /// 加密密碼 (HMACSHA256)
        /// </summary>
        /// <param name="password">明文密碼</param>
        /// <param name="key">加密金鑰 (從 Config 來的)</param>
        /// <returns>Base64 字串</returns>
        public static string EncryptPassword(string password, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = hmac.ComputeHash(passwordBytes);

                // 轉成 Base64 字串回傳
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// 驗證密碼
        /// </summary>
        /// <param name="inputPassword">使用者輸入的明文</param>
        /// <param name="storedHash">資料庫存的雜湊</param>
        /// <param name="key">加密金鑰</param>
        /// <returns>是否正確</returns>
        public static bool VerifyPassword(string inputPassword, string storedHash, string key)
        {
            string encryptedInput = EncryptPassword(inputPassword, key);
            return encryptedInput == storedHash;
        }
    }
}