// <copyright file="ISessionInfo.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    /// Information about session
    /// </summary>
    public interface ISessionInfo
    {
        /// <summary>
        /// Gets session owner id.
        /// </summary>
        string OwnerId { get; }

        /// <summary>
        /// Gets session Id.
        /// </summary>
        [UsedImplicitly]
        string Id { get; }

        /// <summary>
        /// Gets session Creation Timestamp.
        /// </summary>
        [UsedImplicitly]
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets list of URIs to start crawling.
        /// </summary>
        [UsedImplicitly]
        IList<string> RootUris { get; }

        /// <summary>
        /// Gets state of the session.
        /// </summary>
        [UsedImplicitly]
        SessionState State { get; }
    }
}