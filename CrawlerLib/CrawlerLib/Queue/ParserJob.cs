// <copyright file="ParserJob.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Queue
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Parser Job info. What to parse and what to do with links.
    /// </summary>
    public class ParserJob : IParserJob
    {
        private Uri uri;

        /// <summary>
        /// Gets or sets parser crawling depth left.
        /// If zero can parser only this job.
        /// Reduces for going deaper to any pages.
        /// </summary>
        [DataMember(Name = "depth")]
        public int Depth { get; set; }

        /// <summary>
        /// Gets uri host part
        /// </summary>
        [IgnoreDataMember]
        public Uri Host { get; private set; }

        /// <summary>
        /// Gets or sets parser crawling depth left.
        /// If zero can parser only this job.
        /// Reduces for going deaper to pages with different host.
        /// </summary>
        [DataMember(Name = "hostDepth")]
        public int HostDepth { get; set; }

        /// <summary>
        /// Gets or sets crawling session owner id.
        /// </summary>
        [DataMember(Name = "ownerId")]
        public string OwnerId { get; set; }

        /// <summary>
        /// Gets or sets partser parameters.
        /// </summary>
        [DataMember(Name = "parserParameters")]
        public QueueParserParameters ParserParameters { get; set; }

        /// <summary>
        /// Gets or sets referer for URI.
        /// </summary>
        [DataMember(Name = "referrer")]
        public Uri Referrer { get; set; }

        /// <summary>
        /// Gets or sets crawling session id.
        /// </summary>
        [DataMember(Name = "sessionId")]
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets uRI to parse.
        /// </summary>
        [DataMember(Name = "uri")]
        public Uri Uri
        {
            get => uri;
            set
            {
                uri = value;
                Host = new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped));
            }
        }
    }
}