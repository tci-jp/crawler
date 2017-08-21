// <copyright file="PagedSessions.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerApi.Models
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public class Paged<TItem> : IEnumerable<TItem>
    {
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

        [DataMember(Name = "items")]
        public IList<TItem> Items { get; set; }

        [DataMember(Name = "requestId")]
        public string RequestId { get; set; }

        public IEnumerator<TItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Items).GetEnumerator();
        }
    }
}