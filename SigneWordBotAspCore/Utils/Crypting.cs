using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SigneWordBotAspCore.Utils
{
    public static class Crypting
    {
        //TODO: impl this
        /// <summary>
        /// May return null!
        /// </summary>
        /// <param name="input">String that needs to encode</param>
        /// <returns>result of encryption</returns>
        public static string EncryptString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input;
        }
        
        private static readonly byte[] Salt = Encoding.Unicode.GetBytes("7BANANAS");
        private static readonly int Iterations = 2000;

        public static string Encrypt(string plainText, string password)
        {
            byte[] plainBytes = Encoding.Unicode.GetBytes(plainText);
            var aes = Aes.Create();
            var pbkdf2 = new Rfc2898DeriveBytes(password, Salt, Iterations);
            
            
            aes.Key = pbkdf2.GetBytes(32); 
            aes.IV = pbkdf2.GetBytes(16); 

            var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cryptoText, string password)
        {
            var cryptoBytes = Convert.FromBase64String(cryptoText);
            var aes = Aes.Create();
            
            var pbkdf2 = new Rfc2898DeriveBytes(password, Salt,
                Iterations);
            
            aes.Key = pbkdf2.GetBytes(32);
            aes.IV = pbkdf2.GetBytes(16);
            
            var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cryptoBytes, 0, cryptoBytes.Length);
            }

            return Encoding.Unicode.GetString(ms.ToArray());
        }

        //TODO: implement PBKDF2 hash alg http://shawnmclean.com/simplecrypto-net-a-pbkdf2-hashing-wrapper-for-net-framework/

        public static string Sha512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }
    }
}