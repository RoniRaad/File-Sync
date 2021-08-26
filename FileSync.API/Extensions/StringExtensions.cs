using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FileSync.API.Extensions
{
    public static class StringExtensions
    {
        public static byte[] GetHash(this String inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(this String inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in inputString.GetHash())
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        // Because a blob container can only have an lowercase alpha-numeric name with 63 characters maximum we truncate the hash and set all characters to their lowercase form.
        public static string GetBlobContainerUID(this String inputString)
        {
            return inputString.GetHashString()
                    .Substring(0, 63)
                    .ToLower();
        }

        public static string SanitizeDirectory(this String name)
        {
            string regexSearch = new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(name, "");
        }
    }
}