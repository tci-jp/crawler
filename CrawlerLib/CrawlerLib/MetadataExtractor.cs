namespace CrawlerLib
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using HtmlAgilityPack;

    /// <inheritdoc />
    public class MetadataExtractor : IMetadataExtractor
    {
        private static readonly Regex WrongCharRegex = new Regex("[^\\w\\d_]+");

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> ExtractMetadata(HtmlDocument doc)
        {
            foreach (var scope in doc.DocumentNode.SelectNodes("//*[@itemscope]"))
            {
                var itemtype = scope.Attributes["itemtype"]?.Value;
                if (!string.IsNullOrWhiteSpace(itemtype))
                {
                    foreach (var prop in scope.SelectNodes("//*[@itemprop]"))
                    {
                        var itemprop = prop.Attributes["itemprop"]?.Value;
                        if (!string.IsNullOrWhiteSpace(itemprop))
                        {
                            var content = prop.Attributes["datetime"]?.Value
                                          ?? prop.Attributes["content"]?.Value
                                          ?? prop.InnerText;
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                yield return new KeyValuePair<string, string>(
                                    EscapeMetaName(itemtype + "/" + itemprop), content);
                            }
                        }
                    }
                }
            }
        }

        protected virtual string EscapeMetaName(string key)
        {
            const string http = "http://";
            var result = key.StartsWith(http) ? key.Substring(http.Length) : key;
            return WrongCharRegex.Replace(result, "_").Trim('_');
        }
    }
}