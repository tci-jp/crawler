// <copyright file="TableAttribute.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string Table { get; }

        public TableAttribute(string table)
        {
            Table = table;
        }

        public string PartitionKey { get; set; } = null;

        public string RowKey { get; set; } = null;
    }
}
