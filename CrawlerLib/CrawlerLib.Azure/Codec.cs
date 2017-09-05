// <copyright file="Codec.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Azure
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// String encoder and decoder for Azure Table Partition and Row keys.
    /// </summary>
    internal static class Codec
    {
        /// <summary>
        /// Decodes string from Base64.
        /// </summary>
        /// <param name="code">Base64 string.</param>
        /// <returns>Decoded string.</returns>
        public static string DecodeString(string code)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(code));
        }

        /// <summary>
        /// Encodes string into Base64.
        /// </summary>
        /// <param name="str">String to encode.</param>
        /// <returns>Encoded string.</returns>
        public static string EncodeString(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Calculate MD5 Hash for string as string.
        /// </summary>
        /// <param name="s">Input string to hash.</param>
        /// <returns>String with hex MD5.</returns>
        public static string HashString(string s)
        {
            var inputBytes = Encoding.UTF8.GetBytes(s);

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (var i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}