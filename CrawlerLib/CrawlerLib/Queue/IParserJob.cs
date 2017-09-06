// <copyright file="IParserJob.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Queue
{
    using System;
    using System.Runtime.Serialization;

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
        [DataMember(Name = "depth")]
        int Depth { get; }

        /// <summary>
        /// Gets uri host part
        /// </summary>
        [DataMember(Name = "host")]
        Uri Host { get; }

        /// <summary>
        /// Gets parser crawling depth left.
        /// If zero can parser only this job.
        /// Reduces for going deaper to pages with different host.
        /// </summary>
        [DataMember(Name = "hostDepth")]
        int HostDepth { get; }

        /// <summary>
        /// Gets crawling session owner id.
        /// </summary>
        [DataMember(Name = "ownerId")]
        string OwnerId { get; }

        /// <summary>
        /// Gets crawling session id.
        /// </summary>
        [DataMember(Name = "sessionId")]
        string SessionId { get; }

        /// <summary>
        /// Gets referer for URI.
        /// </summary>
        [DataMember(Name = "referrer")]
        Uri Referrer { get; }

        /// <summary>
        /// Gets URI to parse.
        /// </summary>
        [DataMember(Name = "uri")]
        Uri Uri { get; }

        /// <summary>
        /// Gets partser parameters.
        /// </summary>
        [DataMember(Name = "parserParameters")]
        ParserParameters ParserParameters { get;  }
    }
}