// <copyright file="EncodingRedirector.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System.Collections.Generic;
    using System.Text;

    /// <inheritdoc />
    /// <summary>
    /// Provides encodings with alternative names. "UTF-8" as UTF8.
    /// </summary>
    public class EncodingRedirector : EncodingProvider
    {
        private static readonly Dictionary<string, Encoding> Encodings =
            new Dictionary<string, Encoding>
            {
                ["\"UTF-8\""] = Encoding.UTF8
            };

        /// <summary>
        /// Registers encdoing in the system.
        /// </summary>
        public static void RegisterEncodings()
        {
            if ((Encoding.GetEncoding("\"UTF-8\"") == null) && (Encoding.GetEncoding("\"utf-8\"") == null))
            {
                Encoding.RegisterProvider(new EncodingRedirector());
            }
        }

        /// <inheritdoc />
        public override Encoding GetEncoding(string name)
        {
            return Encodings.TryGetValue(name, out var res) ? res : null;
        }

        /// <inheritdoc />
        /// Encoding by codepage number is not supported.
        public override Encoding GetEncoding(int codepage)
        {
            return null;
        }
    }
}