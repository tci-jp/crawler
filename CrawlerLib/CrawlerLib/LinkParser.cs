// <copyright file="LinkParser.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System.Collections.Generic;
    using System.Linq;
    using HtmlAgilityPack;

    /// <inheritdoc />
    public class LinkParser : ILinkParser
    {
        /// <inheritdoc />
        public IEnumerable<string> ParseLinks(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectNodes("//a")
                      ?.Select(l => l.Attributes["href"]).Where(l => l != null)
                      .Select(l => l.Value)
                      .Where(l => !string.IsNullOrWhiteSpace(l)) ?? new List<string>();
        }
    }
}