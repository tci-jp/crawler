// <copyright file="CustomXsltContext.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    /// <summary>
    /// Custom XsltContext with custom fn:replace and fn:match functions
    /// </summary>
    public class CustomXsltContext : XsltContext
    {
        private readonly Dictionary<string, IXsltContextFunction> functions =
            new Dictionary<string, IXsltContextFunction>
            {
                ["fn:replace"] = new XPathRegExReplaceExtensionFunction(),
                ["fn:match"] = new XPathRegExMatchExtensionFunction()
            };

        /// <inheritdoc/>
        public override bool Whitespace => true;

        /// <inheritdoc/>
        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            return 0;
        }

        /// <inheritdoc/>
        public override bool PreserveWhitespace(XPathNavigator node) => true;

        /// <inheritdoc/>
        public override IXsltContextFunction ResolveFunction(
            string prefix,
            string name,
            XPathResultType[] argTypes)
        {
            return functions.TryGetValue(prefix + ":" + name, out var res) ? res : null;
        }

        /// <inheritdoc/>
        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            throw new NotImplementedException();
        }
    }
}