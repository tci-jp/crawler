// <copyright file="RdfaMetadataExtractor.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;

    /// <inheritdoc />
    /// <summary>
    /// Extracts RDFa metadata.
    /// </summary>
    public class RdfaMetadataExtractor : AbstractHtmlMetadataExtractor
    {
        private static readonly Regex PropExtractor = new Regex("([^:]+)$");

        /// <inheritdoc/>
        protected override string PropertyTagName => "property";

        /// <inheritdoc/>
        protected override string ScopeTagName => "vocab";

        /// <inheritdoc/>
        protected override string GetContent(HtmlNode prop)
        {
            return prop.Attributes["datetime"]?.Value
                   ?? prop.Attributes["content"]?.Value
                   ?? prop.InnerText;
        }

        /// <inheritdoc/>
        protected override string GetProperty(HtmlNode prop)
        {
            var itemprop = prop.Attributes["property"]?.Value;
            if (string.IsNullOrWhiteSpace(itemprop))
            {
                return null;
            }

            return PropExtractor.Match(itemprop).Groups[1].Value;
        }

        /// <inheritdoc/>
        protected override string GetScopeType(HtmlNode scope)
        {
            return scope.Attributes["vocab"]?.Value
                   + PropExtractor.Match(scope.Attributes["typeof"]?.Value ?? string.Empty).Groups[1];
        }
    }
}