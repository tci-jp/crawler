// <copyright file="XPathCustomFields.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Queue
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Xpath and field names for custom parsing.
    /// </summary>
    [DataContract]
    public class XPathCustomFields
    {
        /// <summary>
        /// Gets or sets name of schema.org or decode property in URI format
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets xPath expression to extract metadata field from HTML DOM. May include fn:replace and fn:match function for
        /// regular expressions.
        /// </summary>
        [DataMember(Name = "xpath")]
        public string XPath { get; set; }
    }
}