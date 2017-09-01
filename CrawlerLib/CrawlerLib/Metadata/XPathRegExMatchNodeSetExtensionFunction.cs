// <copyright file="XPathRegExMatchNodeSetExtensionFunction.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    /// <summary>
    /// Custom XPath function Match. See <see cref="Regex.Match(string,string)"/>
    /// </summary>
    public class XPathRegExMatchNodeSetExtensionFunction : IXsltContextFunction
    {
        /// <inheritdoc/>
        public XPathResultType[] ArgTypes => new[]
                                             {
                                                 XPathResultType.NodeSet,
                                                 XPathResultType.String
                                             };

        /// <inheritdoc/>
        public int Maxargs => 2;

        /// <inheritdoc/>
        public int Minargs => 2;

        /// <inheritdoc/>
        public XPathResultType ReturnType => XPathResultType.String;

        /// <inheritdoc/>
        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            var result = Enumerate((IEnumerable)args[0], args[1].ToString());
            return string.Join("\0", result);
        }

        private static IEnumerable<string> Enumerate(IEnumerable set, string regex)
        {
            foreach (var node in set)
            {
                var match = Regex.Match(node.ToString(), regex);
                if (match.Success)
                {
                    yield return match.Value;
                }
            }
        }
    }
}