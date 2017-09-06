// <copyright file="UriParameter.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// Uri parameters
    /// </summary>
    [DataContract]
    public class UriParameter : IEquatable<UriParameter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriParameter" /> class.
        /// </summary>
        /// <param name="uri">Uri to crawl.</param>
        /// <param name="depth">Number of links to crawl recursively.</param>
        /// <param name="hostDepth">Number other host to crawl recursively.</param>
        public UriParameter(string uri, int depth = 0, int hostDepth = 0)
        {
            Uri = uri;
            Depth = depth;
            HostDepth = hostDepth;
        }

        /// <summary>
        /// Gets number of links to crawl recursively.
        /// </summary>
        [DataMember(Name = "depth")]
        public int? Depth { get; }

        /// <summary>
        /// Gets number other host to crawl recursively.
        /// </summary>
        [DataMember(Name = "hostDepth")]
        public int? HostDepth { get; }

        /// <summary>
        /// Gets uri to crawl
        /// </summary>
        [DataMember(Name = "uri")]
        [Required]
        public string Uri { get; }

        /// <inheritdoc />
        public bool Equals(UriParameter other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Uri, other.Uri) && (Depth == other.Depth) && (HostDepth == other.HostDepth);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((UriParameter)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Depth.GetHashCode();
                hashCode = (hashCode * 397) ^ HostDepth.GetHashCode();
                hashCode = (hashCode * 397) ^ (Uri != null ? Uri.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}