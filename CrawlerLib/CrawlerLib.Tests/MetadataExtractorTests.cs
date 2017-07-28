// <copyright file="MetadataExtractorTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using System.Linq;
    using HtmlAgilityPack;
    using Xunit;

    public class MetadataExtractorTests
    {
        [Theory]
        [InlineData("metadata-1.html")]
        [InlineData("metadata-2.html")]
        public void TestStability(string filename)
        {
            var html = new HtmlDocument();
            html.Load(filename);
            var extractor = new MetadataExtractor();
            var data = extractor.ExtractMetadata(html).ToList();
        }
    }
}
