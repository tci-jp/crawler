// <copyright file="ILinkParser.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System.Collections.Generic;
    using HtmlAgilityPack;
    using JetBrains.Annotations;

    /// <summary>
    /// HTML doc parser to extract links
    /// </summary>
    public interface ILinkParser
    {
        /// <summary>
        /// Returns links in HTML.
        /// </summary>
        /// <param name="doc">HTML document.</param>
        /// <returns>Links. Not Null.</returns>
        [NotNull]
        IEnumerable<string> ParseLinks(HtmlDocument doc);
    }
}