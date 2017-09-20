// <copyright file="IParserParameters.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Parameters for parsing web pages.
    /// </summary>
    public interface IParserParameters
    {
        /// <summary>
        /// Gets custom parsing fields
        /// </summary>
        [DataMember(Name = "customFields")]
        IEnumerable<IParserParametersXPathCustomFields> CustomFields { get; }

        /// <summary>
        /// Gets owner id
        /// </summary>
        [DataMember(Name = "ownerId")]
        string OwnerId { get; }

        /// <summary>
        /// Gets parser id in free text unique for specified owner
        /// </summary>
        [DataMember(Name = "parserId")]
        string ParserId { get; }

        /// <summary>
        /// Gets use ld+json metadata
        /// </summary>
        [DataMember(Name = "useJson")]
        bool? UseJson { get; }

        /// <summary>
        /// Gets use Microdata attributes for parsing metadata
        /// </summary>
        [DataMember(Name = "useMicrodata")]
        bool? UseMicrodata { get; }

        /// <summary>
        /// Gets use RDFa attributes for parsing metadata.
        /// </summary>
        [DataMember(Name = "useRdfa")]
        bool? UseRdFa { get; }
    }
}