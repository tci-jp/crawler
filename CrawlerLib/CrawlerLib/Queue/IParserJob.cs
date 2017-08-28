// <copyright file="IParserJob.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System;

    /// <summary>
    /// General parser job description.
    /// </summary>
    public interface IParserJob
    {
        /// <summary>
        /// Gets parser crawling depth left.
        /// If zero can parser only this job.
        /// Reduces for going deaper to any pages.
        /// </summary>
        int Depth { get; }

        /// <summary>
        /// Gets uri host part
        /// </summary>
        Uri Host { get; }

        /// <summary>
        /// Gets parser crawling depth left.
        /// If zero can parser only this job.
        /// Reduces for going deaper to pages with different host.
        /// </summary>
        int HostDepth { get; }

        /// <summary>
        /// Gets crawling session owner id.
        /// </summary>
        string OwnerId { get; }

        /// <summary>
        /// Gets crawling session id.
        /// </summary>
        string SessionId { get; }

        /// <summary>
        /// Gets referer for URI.
        /// </summary>
        Uri Referrer { get; }

        /// <summary>
        /// Gets URI to parse.
        /// </summary>
        Uri Uri { get; }
    }
}