// <copyright file="XPathMetadataExtractor.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using System.Collections.Generic;
    using System.Xml.XPath;
    using HtmlAgilityPack;

    /// <summary>
    /// Metadata extractor using extended xpath expressions.
    /// Extension includes fn:replace and fn:match functions
    /// which work the same as Regex functions with the same name.
    /// </summary>
    public class XPathMetadataExtractor : IMetadataExtractor
    {
        private readonly CustomXsltContext context = new CustomXsltContext();
        private readonly IReadOnlyList<KeyValuePair<XPathExpression, string>> xpaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathMetadataExtractor" /> class.
        /// </summary>
        /// <param name="xpaths">Collection of pairs of xpath expressions and field names.</param>
        public XPathMetadataExtractor(IEnumerable<KeyValuePair<string, string>> xpaths)
        {
            var expressions = new List<KeyValuePair<XPathExpression, string>>();
            foreach (var pair in xpaths)
            {
                var exp = XPathExpression.Compile(pair.Key);
                exp.SetContext(context);

                expressions.Add(new KeyValuePair<XPathExpression, string>(exp, pair.Value));
            }

            this.xpaths = expressions;
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> ExtractMetadata(HtmlDocument doc)
        {
            var nav = doc.CreateNavigator();
            foreach (var pair in xpaths)
            {
                var key = pair.Value;
                var value = nav.Evaluate(pair.Key)?.ToString();
                if (value != null)
                {
                    yield return new KeyValuePair<string, string>(key, value);
                }
            }
        }
    }
}