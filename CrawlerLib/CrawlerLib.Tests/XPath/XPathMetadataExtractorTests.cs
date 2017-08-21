// <copyright file="XPathMetadataExtractorTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests.XPath
{
    using System.Collections.Generic;
    using System.Linq;
    using HtmlAgilityPack;
    using Metadata;
    using Xunit;

    public class XPathMetadataExtractorTests
    {
        private HtmlDocument doc;

        public XPathMetadataExtractorTests()
        {
            doc = new HtmlDocument();
            doc.LoadHtml("<body><div id=\"asdf1234qwer\"/></body>");
        }

        [Theory]
        [InlineData("string(//div/@id)", "asdf1234qwer")]
        [InlineData("fn:match(string(//div/@id),'(\\d+)')", "1234")]
        public void TestXPath(string xpath, string result)
        {
            var extractor = new XPathMetadataExtractor(new[] { new KeyValuePair<string, string>(xpath, "field") });
            Assert.Equal(result, extractor.ExtractMetadata(doc).Single().Value);
        }
    }
}
