// <copyright file="JsonMetadataExtractor.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HtmlAgilityPack;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// ld+Json metadata extractor.
    /// </summary>
    public class JsonMetadataExtractor : IMetadataExtractor
    {
        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> ExtractMetadata(HtmlDocument doc)
        {
            var jsons = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
            if (jsons != null)
            {
                foreach (var json in jsons)
                {
                    var root = JObject.Parse(json.InnerText);
                    var context = (root["@context"] as JValue)?.ToString();
                    if (context != null)
                    {
                        foreach (var node in root.SelectTokens("$..[?(@.@type)]").OfType<JObject>())
                        {
                            var typ = node["@type"]?.ToString();
                            if (typ != null)
                            {
                                foreach (var meta in Process(node, context + "/" + typ))
                                {
                                    yield return meta;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> Process(JObject obj, string context)
        {
            foreach (var prop in obj.Properties().Where(p => !p.Name.StartsWith("@") && (p.Value != null)))
            {
                var propValue = prop.Value;
                var newcontext = context + "/" + prop.Name;
                switch (propValue)
                {
                    case JValue val:
                        yield return new KeyValuePair<string, string>(newcontext, val.ToString());
                        break;
                    case JObject subobj:
                        foreach (var meta in Process(subobj, newcontext))
                        {
                            yield return meta;
                        }

                        break;
                }
            }
        }
    }
}