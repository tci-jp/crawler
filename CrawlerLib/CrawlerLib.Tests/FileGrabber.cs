// <copyright file="FileGrabber.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>
namespace CrawlerLib.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using CrawlerLib.Grabbers;

    public class FileGrabber : HttpGrabber
    {
        public FileGrabber(Configuration config)
            : base(config)
        {
        }

        /// <inheritdoc />
        public override async Task<GrabResult> Grab(Uri uri, Uri referer)
        {
            var filePath = "index.html";
            if (uri.PathAndQuery != "/")
            {
                filePath = uri.LocalPath.Trim('/');
            }

            filePath = Path.Combine(Directory.GetCurrentDirectory(), uri.Host, filePath);
            if (File.Exists(filePath))
            {
                var content = await File.ReadAllTextAsync(filePath);
                return new GrabResult { Status = HttpStatusCode.OK, Content = content };
            }

            return new GrabResult { Status = HttpStatusCode.NotFound, Content = string.Empty };
        }
    }
}