// <copyright file="IMetadataExtractor.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using System.Collections.Generic;
    using System.Threading;
    using HtmlAgilityPack;

    /// <summary>
    /// Extracts Schema.org-like metadata
    /// </summary>
    public interface IMetadataExtractor
    {
        /// <summary>
        /// Extracts Schema.org-like metadata
        /// </summary>
        /// <param name="doc">HTML to process.</param>
        /// <param name="cancellation">Processing cancellation.</param>
        /// <returns>
        /// Collections of metadata name and value pairs.
        /// Can be more than one metadata value for same name.
        /// </returns>
        IEnumerable<KeyValuePair<string, string>> ExtractMetadata(HtmlDocument doc, CancellationToken cancellation);
    }
}