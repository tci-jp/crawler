// <copyright file="Paged.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerApi.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;
    using JetBrains.Annotations;

    /// <summary>
    /// Pagination container.
    /// </summary>
    /// <typeparam name="TItem">Page items type.</typeparam>
    [DataContract]
    public class Paged<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Paged{TItem}" /> class.
        /// </summary>
        /// <param name="items">Page items.</param>
        /// <param name="requestId">Continuation for pagination.</param>
        public Paged(IEnumerable<TItem> items, string requestId)
        {
            RequestId = requestId;
            if (items is IList<TItem> list)
            {
                Items = list;
            }
            else
            {
                Items = new List<TItem>(items);
            }
        }

        /// <summary>
        /// Gets or sets page items.
        /// </summary>
        [DataMember(Name = "items")]
        [UsedImplicitly]
        [Required]
        public IList<TItem> Items { get; set; }

        /// <summary>
        /// Gets or sets pagination continuation token.
        /// </summary>
        [DataMember(Name = "requestId")]
        [UsedImplicitly]
        public string RequestId { get; set; }
    }
}