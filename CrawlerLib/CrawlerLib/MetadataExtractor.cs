// <copyright file="MetadataExtractor.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;

    /// <inheritdoc />
    public class MetadataExtractor : IMetadataExtractor
    {
        private static readonly Regex PropExtractor = new Regex("([^:]+)$");

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> ExtractMetadata(HtmlDocument doc)
        {
            return ExtractHtmlMetadata(
                    doc,
                    "itemtype",
                    scope => scope.Attributes["itemtype"]?.Value,
                    "itemprop",
                    prop => prop.Attributes["itemprop"]?.Value,
                    GetContent)
                .Concat(
                    ExtractHtmlMetadata(
                        doc,
                        "vocab",
                        scope => scope.Attributes["vocab"]?.Value
                                 + PropExtractor.Match(scope.Attributes["typeof"]?.Value ?? string.Empty).Groups[1]
                                                .Value,
                        "property",
                        prop => prop.Attributes["property"]?.Value,
                        GetContent));
        }

        private string GetContent(HtmlNode prop)
        {
            return prop.Attributes["datetime"]?.Value
                   ?? prop.Attributes["content"]?.Value
                   ?? prop.InnerText;
        }

        private IEnumerable<KeyValuePair<string, string>> ExtractHtmlMetadata(
            HtmlDocument doc,
            string scopetag,
            Func<HtmlNode, string> getType,
            string proptag,
            Func<HtmlNode, string> getProp,
            Func<HtmlNode, string> getContent)
        {
            var itempropXPath = $".//*[string(@{proptag}) and not(*)]";
            foreach (var scope in doc.DocumentNode.SelectNodes($"//*[string(@{scopetag})]")
                                  ?? Enumerable.Empty<HtmlNode>())
            {
                var itemtype = getType(scope);
                if (!string.IsNullOrWhiteSpace(itemtype))
                {
                    foreach (var prop in scope.SelectNodes(itempropXPath)
                                         ?? Enumerable.Empty<HtmlNode>())
                    {
                        var itemprop = getProp(prop);
                        if (!string.IsNullOrWhiteSpace(itemprop))
                        {
                            var content = getContent(prop);

                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                itemprop = PropExtractor.Match(itemprop).Groups[1].Value;
                                yield return new KeyValuePair<string, string>(
                                    itemtype + "/" + GetFullPropName(scope, prop, itemprop, getProp),
                                    content);
                            }
                        }
                    }
                }
            }
        }

        private object GetFullPropName(
            HtmlNode root,
            HtmlNode prop,
            string lastitemprop,
            Func<HtmlNode, string> getProp)
        {
            var keys = new Stack<string>();
            keys.Push(lastitemprop);
            var curprop = prop;
            var rootpath = root.XPath;
            while (curprop.XPath != rootpath)
            {
                curprop = curprop.ParentNode;
                var itemprop = getProp(curprop);
                if (!string.IsNullOrWhiteSpace(itemprop))
                {
                    itemprop = PropExtractor.Match(itemprop).Groups[1].Value;
                    keys.Push(itemprop);
                }
            }

            return string.Join("/", keys);
        }
    }
}