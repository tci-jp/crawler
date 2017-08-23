// <copyright file="Page.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Page of items
    /// </summary>
    /// <typeparam name="TItem">Type of items.</typeparam>
    public class Page<TItem> : IPage<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Page{TItem}" /> class.
        /// </summary>
        /// <param name="items">Items.</param>
        /// <param name="requestId">Paged request id.</param>
        public Page(IEnumerable<TItem> items, string requestId)
        {
            Items = items;
            RequestId = requestId;
        }

        /// <inheritdoc />
        public IEnumerable<TItem> Items { get; }

        /// <inheritdoc />
        public string RequestId { get; }
    }
}