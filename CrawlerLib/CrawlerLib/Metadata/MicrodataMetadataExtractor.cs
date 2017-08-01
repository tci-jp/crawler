// <copyright file="MicrodataMetadataExtractor.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using HtmlAgilityPack;

    /// <inheritdoc />
    /// <summary>
    /// Extracts Microdata metadata.
    /// </summary>
    public class MicrodataMetadataExtractor : AbstractHtmlMetadataExtractor
    {
        /// <inheritdoc/>
        protected override string PropertyTagName => "itemprop";

        /// <inheritdoc/>
        protected override string ScopeTagName => "itemtype";

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
            return prop.Attributes["itemprop"]?.Value;
        }

        /// <inheritdoc/>
        protected override string GetScopeType(HtmlNode scope)
        {
            return scope.Attributes["itemtype"]?.Value;
        }
    }
}