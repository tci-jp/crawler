// <copyright file="XPathMetadataExtractorTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests.XPath
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using FluentAssertions;
    using HtmlAgilityPack;
    using Metadata;
    using Queue;
    using Xunit;

    public class XPathMetadataExtractorTests
    {
        [Theory]
        [InlineData("<body><div id=\"asdf1234qwer\"/></body>", "string(//div/@id)", new[] { "asdf1234qwer" })]
        [InlineData("<body><div id=\"asdf1234qwer\"/></body>", "fn:match(string(//div/@id),'(\\d+)')", new[] { "1234" })]
        [InlineData("<body><div id=\"asdf1234qwer\"/><div id=\"asdf097qwer\"/></body>", "fn:match(//div/@id,'(\\d+)')", new[] { "1234", "097" })]
        public void TestXPath(string doccontent, string xpath, string[] result)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(doccontent);
            var extractor = new XPathMetadataExtractor(new[] { new XPathCustomFields { XPath = xpath, Name = "field" } });
            var meta = extractor.ExtractMetadata(doc, CancellationToken.None);
            meta.Select(m => m.Value).ShouldBeEquivalentTo(result);
        }
    }
}
