﻿// <copyright file="IPage.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Page of items
    /// </summary>
    /// <typeparam name="TItem">Type of items.</typeparam>
    public interface IPage<out TItem> : IEnumerable<TItem>
    {
        /// <summary>
        /// Gets paged request id for other pages
        /// </summary>
        string RequestId { get; }
    }
}