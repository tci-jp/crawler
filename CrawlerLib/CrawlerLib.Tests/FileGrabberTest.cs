// <copyright file="FileGrabberTest.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>
namespace CrawlerLib.Tests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Xunit;

    public class FileGrabberTest
    {
        [Fact]
        public async Task SuccessCheck()
        {
            var config = new Configuration();
            var grab = new FileGrabber(config);
            var res = await grab.Grab(new Uri("http://filegrabber/sample.txt"), null);
            Assert.Equal(expected: "TestContent", actual: res.Content);
            Assert.Equal(expected: HttpStatusCode.OK, actual: res.Status);
        }

        [Fact]
        public async Task FailureCheck()
        {
            var config = new Configuration();
            var grab = new FileGrabber(config);
            var res = await grab.Grab(new Uri("http://filegrabber/sample.txtt"), null);
            Assert.Equal(expected: string.Empty, actual: res.Content);
            Assert.Equal(expected: HttpStatusCode.NotFound, actual: res.Status);
        }
    }
}
