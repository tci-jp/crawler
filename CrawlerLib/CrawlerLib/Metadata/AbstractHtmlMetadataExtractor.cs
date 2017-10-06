// <copyright file="AbstractHtmlMetadataExtractor.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using HtmlAgilityPack;

    /// <inheritdoc />
    /// <summary>
    /// Abstract class to implement similar metadata markings based on attributes in html tags.
    /// </summary>
    public abstract class AbstractHtmlMetadataExtractor : IMetadataExtractor
    {
        /// <summary>
        /// Gets name of attribute containing metadata property name.
        /// </summary>
        protected abstract string PropertyTagName { get; }

        /// <summary>
        /// Gets name of attruibute marking html tag as metadata object root.
        /// </summary>
        protected abstract string ScopeTagName { get; }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> ExtractMetadata(HtmlDocument doc, CancellationToken cancellation)
        {
            var itempropXPath = $".//*[string(@{PropertyTagName}) and not(*)]";
            foreach (var scope in doc.DocumentNode.SelectNodes($"//*[string(@{ScopeTagName})]")
                                  ?? Enumerable.Empty<HtmlNode>())
            {
                cancellation.ThrowIfCancellationRequested();
                var itemtype = GetScopeType(scope);
                if (!string.IsNullOrWhiteSpace(itemtype))
                {
                    foreach (var prop in scope.SelectNodes(itempropXPath)
                                         ?? Enumerable.Empty<HtmlNode>())
                    {
                        cancellation.ThrowIfCancellationRequested();
                        var itemprop = GetProperty(prop);
                        if (!string.IsNullOrWhiteSpace(itemprop))
                        {
                            var content = GetContent(prop);

                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                yield return new KeyValuePair<string, string>(
                                    itemtype + "/" + GetFullPropName(scope, prop, itemprop),
                                    content);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extracts metadata value content from node.
        /// </summary>
        /// <param name="prop">Html node to extract from.</param>
        /// <returns>Metadata value.</returns>
        protected abstract string GetContent(HtmlNode prop);

        /// <summary>
        /// Extracts metadata property name from node.
        /// </summary>
        /// <param name="prop">Node to extract from.</param>
        /// <returns>Proprerty name.</returns>
        protected abstract string GetProperty(HtmlNode prop);

        /// <summary>
        /// Extracts metadata scope and type from node.
        /// </summary>
        /// <param name="scope">Node to extract from.</param>
        /// <returns>Metadata scope and type. Example value: http://schema.org/Thing </returns>
        protected abstract string GetScopeType(HtmlNode scope);

        private string GetFullPropName(
            HtmlNode root,
            HtmlNode prop,
            string lastitemprop)
        {
            var keys = new Stack<string>();
            keys.Push(lastitemprop);
            var curprop = prop;
            var rootpath = root.XPath;
            while (curprop.XPath != rootpath)
            {
                curprop = curprop.ParentNode;
                var itemprop = GetProperty(curprop);
                if (!string.IsNullOrWhiteSpace(itemprop))
                {
                    keys.Push(itemprop);
                }
            }

            return string.Join("/", keys);
        }
    }
}