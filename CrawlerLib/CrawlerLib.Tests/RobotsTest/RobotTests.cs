// <copyright file="RobotsTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Tests
{
    using HtmlAgilityPack;
    using Xunit;

    public class RobotTests
    {
        [Theory]

         [InlineData("Stupidbot", @"RobotsTest/Robots1.txt", "/Data/", false)]
         [InlineData("Google", @"RobotsTest/Robots1.txt", "/Grabber/", false)]
         [InlineData("Badbot", @"RobotsTest/Robots1.txt", "/Logger/", false)]
         [InlineData("nvqtu", @"RobotsTest/Robots1.txt", "/Metadata/", false)]
         [InlineData("Stupidbot", @"RobotsTest/Robots2.txt", "/Data/", true)]
         [InlineData("Google", @"RobotsTest/Robots2.txt", "/Grabber/", true)]
         [InlineData("Badbot", @"RobotsTest/Robots2.txt", "/Logger/", true)]
         [InlineData("nvqtu", @"RobotsTest/Robots2.txt", "/Metadata/", true)]
         [InlineData("Stupidbot", "RobotsTest/Robots3.txt", "/Data/", false)]
         [InlineData("Google", @"RobotsTest/Robots3.txt", "/Grabber/", false)]
         [InlineData("Badbot", @"RobotsTest/Robots3.txt", "/Logger/", false)]
         [InlineData("nvqtu", @"RobotsTest/Robots3.txt", "/Metadata/", true)]
         [InlineData("Stupidbot", @"RobotsTest/Robots4.txt", "/Data/", false)]
         [InlineData("Google", @"RobotsTest/Robots4.txt", "/Grabber/", true)]
         [InlineData("Badbot", @"RobotsTest/Robots4.txt", "/Logger/", false)]
         [InlineData("nvqtu", @"RobotsTest/Robots4.txt", "/Metadata/", false)]
        public void TestURIAllowed(string userAgent, string pathRobotFiles, string url, bool result)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(pathRobotFiles);
            var content = file.ReadToEnd();
            var bot = new Robots(userAgent, content);
            Assert.Equal(result, bot.IsPathAllowed(url));
        }
    }
}
