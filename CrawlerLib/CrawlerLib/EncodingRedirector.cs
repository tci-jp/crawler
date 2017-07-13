// <copyright file="EncodingRedirector.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class EncodingRedirector : EncodingProvider
    {
        private static readonly Dictionary<string, Encoding> encodings = new Dictionary<string, Encoding>
                                                                         {
                                                                             ["\"UTF-8\""] = Encoding.UTF8
                                                                         };

        /// <inheritdoc />
        public override Encoding GetEncoding(string name)
        {
            return encodings.TryGetValue(name, out var res) ? res : null;
        }

        /// <inheritdoc />
        public override Encoding GetEncoding(int codepage)
        {
            return null;
        }

        public static void RegisterEncodings()
        {
            if (!Encoding.GetEncodings()
                         .Any(e => e.Name.Equals("\"UTF-8\"", StringComparison.InvariantCultureIgnoreCase)))
            {
                Encoding.RegisterProvider(new EncodingRedirector());
            }
        }
    }
}