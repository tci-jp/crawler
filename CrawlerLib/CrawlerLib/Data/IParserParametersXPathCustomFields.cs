// <copyright file="IParserParametersXPathCustomFields.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    /// <summary>
    /// XPath to metadata mapping for custop parsing.
    /// </summary>
    public interface IParserParametersXPathCustomFields
    {
        /// <summary>
        /// Gets name of schema.org or decode property in URI format
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets xPath expression to extract metadata field from HTML DOM. May include fn:replace and fn:match function for regular
        /// expressions.
        /// </summary>
        string XPath { get; }
    }
}