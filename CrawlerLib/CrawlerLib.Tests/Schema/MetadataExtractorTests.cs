// <copyright file="MetadataExtractorTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests.Schema
{
    using System.Linq;
    using HtmlAgilityPack;
    using Metadata;
    using Xunit;

    public class MetadataExtractorTests
    {
        [Theory]
        [InlineData("Schema/metadata-1.html")]
        public void TestMicrodataStability(string filename)
        {
            var html = new HtmlDocument();
            html.Load(filename);
            var extractor = new MicrodataMetadataExtractor();
            var data = extractor.ExtractMetadata(html).ToList();
        }

        [Theory]
        [InlineData("Schema/metadata-2.html")]
        public void TestRdfaStability(string filename)
        {
            var html = new HtmlDocument();
            html.Load(filename);
            var extractor = new RdfaMetadataExtractor();
            var data = extractor.ExtractMetadata(html).ToList();
        }
    }
}
