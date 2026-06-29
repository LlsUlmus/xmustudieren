using System.Security.Cryptography;

namespace Ricebird.Framework
{
    public static class SecureHelper
    {
        public static string GetSha1(string inputStr)
        {
            byte[] cleanBytes = Encoding.Default.GetBytes(inputStr);
            byte[] hashedBytes = SHA1.HashData(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        public static string GetSha256(string inputStr)
        {
            byte[] cleanBytes = Encoding.Default.GetBytes(inputStr);
            byte[] hashedBytes = SHA256.HashData(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// MD5散列
        /// </summary>
        public static string MD5(byte[] bytes)
        {
            byte[] hashByte = System.Security.Cryptography.MD5.HashData(bytes);
            StringBuilder sb = new();
            foreach (byte item in hashByte)
                sb.Append(item.ToString("x").PadLeft(2, '0'));
            return sb.ToString().ToLower();
        }

        private readonly static char[] errorChars = [];
        public static string ToSafeFileName(this string str)
        {
            StringBuilder builder = new StringBuilder();

            foreach (var c in str)
            {
                switch (c)
                {
                    case '/':
                    case '\\':
                        builder.Append('-');
                        break;
                    case ':':
                    case '*':
                    case '?':
                    case '"':
                    case '<':
                    case '>':
                    case '|':
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
