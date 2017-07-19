// <copyright file="SessionInfo.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System;
    using System.Collections.Generic;

    public interface ISessionInfo
    {
        string Id { get; }

        DateTime Timestamp { get; }

        IList<string> RootUris { get; }
    }
}