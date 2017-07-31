// <copyright file="SearchOperatorsModel.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CrawlerLib;
    using CrawlerLib.Data;

    /// <summary>
    /// Model to hold all available query comparison operators.
    /// </summary>
    public class SearchOperatorsModel
    {
        /// <summary>
        /// Gets available query comparison operators.
        /// </summary>
        public IList<SearchCondition.Operator> Items { get; } =
            Enum.GetValues(typeof(SearchCondition.Operator)).Cast<SearchCondition.Operator>().ToList();
    }
}