// <copyright file="MetadataExtractorTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests.Schema
{
    using System.Linq;
    using FluentAssertions;
    using HtmlAgilityPack;
    using Metadata;
    using Xunit;

    public class MetadataExtractorTests
    {
        [Theory]
        [InlineData("Schema/metadata-3.html", new[]
                                              {
                                                  "http://schema.org/Event/location/address/addressLocality",
                                                  "http://schema.org/Event/location/name",
                                                  "http://schema.org/Event/name",
                                                  "http://schema.org/Place/address/addressLocality",
                                                  "http://schema.org/Place/name",
                                                  "http://schema.org/PostalAddress/addressLocality"
                                              })]
        public void TestJsonMetadata(string filename, string[] keys)
        {
            var html = new HtmlDocument();
            html.Load(filename);
            var extractor = new JsonMetadataExtractor();
            var data = extractor.ExtractMetadata(html).Select(p => p.Key).ToList();
            data.ShouldBeEquivalentTo(keys);
        }

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