// <copyright file="SessionInfo.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System;
    using System.Collections.Generic;

    public class SessionInfo
    {
        public string Id { get; set; }

        public DateTime Timestamp { get; set; }

        public IList<string> RootUris { get; set; }
    }
}