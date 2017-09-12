// <copyright file="ParserParameters.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Queue
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Metadata;

    /// <summary>
    /// Parameters for parsing pages.
    /// </summary>
    [DataContract]
    public class ParserParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether parser should use Microdata attributes for parsing metadata
        /// </summary>
        [DataMember(Name = "useMicrodata")]
        public bool UseMicrodata { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether parser should use RDFa attributes for parsing metadata.
        /// </summary>
        [DataMember(Name = "useRDFa")]
        public bool UseRdFa { get; set; }

        /// <summary>
        /// Gets or sets CustomFields
        /// </summary>
        [DataMember(Name = "xpathCustomFields")]
        public IList<XPathCustomFields> XPathCustomFields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether parser should use ld+json metadata.
        /// </summary>
        [DataMember(Name = "useJson")]
        public bool UseJson { get; set; }

        /// <summary>
        /// Build enumeration with extractors.
        /// </summary>
        /// <returns>Extractors.</returns>
        public IEnumerable<IMetadataExtractor> GetExtractors()
        {
            if (UseJson)
            {
                yield return new JsonMetadataExtractor();
            }

            if (UseRdFa)
            {
                yield return new RdfaMetadataExtractor();
            }

            if (UseMicrodata)
            {
                yield return new MicrodataMetadataExtractor();
            }

            if (XPathCustomFields != null)
            {
                yield return new XPathMetadataExtractor(XPathCustomFields);
            }
        }
    }
}