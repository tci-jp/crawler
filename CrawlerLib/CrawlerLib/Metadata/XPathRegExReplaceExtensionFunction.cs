// <copyright file="XPathRegExReplaceExtensionFunction.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using System.Text.RegularExpressions;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    /// <summary>
    /// Custom XPath function Replace. See <see cref="Regex.Replace(string, string, string)"/>
    /// </summary>
    public class XPathRegExReplaceExtensionFunction : IXsltContextFunction
    {
        /// <inheritdoc/>
        public XPathResultType[] ArgTypes => new[]
                                             {
                                                 XPathResultType.String, XPathResultType.String,
                                                 XPathResultType.String
                                             };

        /// <inheritdoc/>
        public int Maxargs => 3;

        /// <inheritdoc/>
        public int Minargs => 3;

        /// <inheritdoc/>
        public XPathResultType ReturnType => XPathResultType.String;

        /// <inheritdoc/>
        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            return Regex.Replace(args[0].ToString(), args[1].ToString(), args[2].ToString());
        }
    }
}