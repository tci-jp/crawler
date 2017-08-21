// <copyright file="Page.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerLib.Data
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Page of items
    /// </summary>
    /// <typeparam name="TItem">Type of items.</typeparam>
    public class Page<TItem> : IPage<TItem>
    {
        private readonly IEnumerable<TItem> pageImplementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="Page{TItem}" /> class.
        /// </summary>
        /// <param name="items">Items.</param>
        /// <param name="requestId">Paged request id.</param>
        public Page(IEnumerable<TItem> items, string requestId)
        {
            pageImplementation = items;
            RequestId = requestId;
        }

        /// <inheritdoc />
        public string RequestId { get; }

        /// <inheritdoc />
        public IEnumerator<TItem> GetEnumerator()
        {
            return pageImplementation.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)pageImplementation).GetEnumerator();
        }
    }
}