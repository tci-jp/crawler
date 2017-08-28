// <copyright file="RobotstxtTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests.Robotstxt
{
    using System.IO;
    using Xunit;

    public class RobotstxtTests
    {
        [Theory]
        [InlineData("Stupidbot", @"Robotstxt/Robots1.txt", "/Data/", false)]
        [InlineData("Google", @"Robotstxt/Robots1.txt", "/Grabber/", false)]
        [InlineData("Badbot", @"Robotstxt/Robots1.txt", "/Logger/", false)]
        [InlineData("nvqtu", @"Robotstxt/Robots1.txt", "/Metadata/", false)]
        [InlineData("Stupidbot", @"Robotstxt/Robots2.txt", "/Data/", true)]
        [InlineData("Google", @"Robotstxt/Robots2.txt", "/Grabber/", true)]
        [InlineData("Badbot", @"Robotstxt/Robots2.txt", "/Logger/", true)]
        [InlineData("nvqtu", @"Robotstxt/Robots2.txt", "/Metadata/", true)]
        [InlineData("Stupidbot", "Robotstxt/Robots3.txt", "/Data/", false)]
        [InlineData("Google", @"Robotstxt/Robots3.txt", "/Grabber/", false)]
        [InlineData("Badbot", @"Robotstxt/Robots3.txt", "/Logger/", false)]
        [InlineData("nvqtu", @"Robotstxt/Robots3.txt", "/Metadata/", true)]
        [InlineData("Stupidbot", @"Robotstxt/Robots4.txt", "/Data/", false)]
        [InlineData("Google", @"Robotstxt/Robots4.txt", "/Grabber/", true)]
        [InlineData("Badbot", @"Robotstxt/Robots4.txt", "/Logger/", false)]
        [InlineData("nvqtu", @"Robotstxt/Robots4.txt", "/Metadata/", false)]
        public void TestUriAllowed(string userAgent, string pathRobotFiles, string url, bool result)
        {
            var content = File.ReadAllText(pathRobotFiles);
            var bot = new Robots(userAgent, content);
            Assert.Equal(result, bot.IsPathAllowed(url));
        }
    }
}