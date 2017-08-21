// <copyright file="TableAttribute.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// Defines entity Table name and optional fixed Partition and Row keys.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableAttribute" /> class.
        /// </summary>
        /// <param name="tableName">Name of Table.</param>
        public TableAttribute(string tableName)
        {
            Table = tableName;
        }

        /// <summary>
        /// Gets table name for entities.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// Gets or sets optional Partition Key
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets optional Row Key.
        /// </summary>
        public string RowKey { get; set; } = null;
    }
}