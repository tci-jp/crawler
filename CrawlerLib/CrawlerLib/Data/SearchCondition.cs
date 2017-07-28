// <copyright file="SearchCondition.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib
{
    using System.Collections.Generic;

    /// <summary>
    /// Search condition operator
    /// </summary>
    public class SearchCondition
    {
        /// <summary>
        /// Comparision operator
        /// </summary>
        public enum Operator
        {
            /// <summary>
            /// Equal
            /// </summary>
            Equal,

            /// <summary>
            /// Not equal
            /// </summary>
            NotEqual
        }

        /// <summary>
        /// Gets or sets metadata field name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets metadata value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets comparison operator
        /// </summary>
        public Operator Op { get; set; }
    }
}