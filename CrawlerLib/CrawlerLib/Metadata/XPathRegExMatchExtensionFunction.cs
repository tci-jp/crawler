// <copyright file="XPathRegExMatchExtensionFunction.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;
    using HtmlAgilityPack;

    /// <summary>
    /// Custom XPath function Match. See <see cref="Regex.Match(string, string)"/>
    /// </summary>
    public class XPathRegExMatchExtensionFunction : IXsltContextFunction
    {
        /// <inheritdoc/>
        public XPathResultType[] ArgTypes => new[]
                                             {
                                                 XPathResultType.String,
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
            return Regex.Match(args[0].ToString(), args[1].ToString());
        }
    }
}