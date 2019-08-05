namespace SigneWordBotAspCore.Base
{
    internal static class Crypting
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