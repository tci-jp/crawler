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
            var uriPath = uri.LocalPath;
            var filePath = Directory.GetCurrentDirectory() + uriPath;
            if (!uri.AbsoluteUri.StartsWith("file://"))
            {
                filePath = Directory.GetCurrentDirectory() + uri.AbsoluteUri.Replace("http://", "\\").Replace("/", "\\");
                if (uri.AbsoluteUri.Replace("http://", "\\").Replace("/", "\\").LastOrDefault() == '\\')
                {
                    filePath += "index.html";
                }
                else
                {
                    filePath += "\\index.html";
                }
            }

            GrabResult grabResult = null;
            if (File.Exists(filePath))
            {
                string content = await File.ReadAllTextAsync(filePath);
                grabResult = new GrabResult { Status = HttpStatusCode.OK, Content = content };
            }
            else
            {
                grabResult = new GrabResult { Status = HttpStatusCode.NotFound, Content = null };
            }

            return grabResult;
        }
    }
}