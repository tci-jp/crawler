// <copyright file="TestTableEntity.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage.Tests
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Xunit;

    public class TestTableEntity : TableEntity
    {
        public bool BoolField { get; set; }

        public float FloatField { get; set; }

        public int IntField { get; set; }

        [PartitionKey]
        public string PartKey => PartitionKey;

        [RowKey]
        public string RKey => RowKey;

        public string StringField { get; set; }
    }
}