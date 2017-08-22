// <copyright file="TableQueryExtensionsTests.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage.Tests
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Xunit;

    public class TableQueryExtensionsTests
    {
        private readonly TableQuery<TestTableEntity> baseQuery = new TableQuery<TestTableEntity>();

        /// <summary>
        /// BoolField
        /// </summary>
        [Fact]
        public void TestEqualBoolField()
        {
            var testQuery = baseQuery.Where(e => e.BoolField == true);
            Assert.Equal("BoolField eq true", testQuery.FilterString);
        }

        [Fact]
        public void TestNotEqualBoolField()
        {
            var testQuery = baseQuery.Where(e => e.BoolField != true);
            Assert.Equal("BoolField ne true", testQuery.FilterString);
        }

        [Fact]
        public void TestAndBoolField()
        {
            var testQuery = baseQuery.Where(e => (e.BoolField == true && e.BoolField != true));
            var xxx = testQuery.FilterString;
            Assert.Equal("(BoolField eq true) and (BoolField ne true)", testQuery.FilterString);
        }

        [Fact]
        public void TestOrBoolField()
        {
            var testQuery = baseQuery.Where(e => (e.BoolField == true || e.BoolField != true));
            var xxx = testQuery.FilterString;
            Assert.Equal("(BoolField eq true) or (BoolField ne true)", testQuery.FilterString);
        }

        /// <summary>
        /// FloatField
        /// </summary>
        [Fact]
        public void TestLtFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField < 1));
            Assert.Equal("FloatField lt 1.0", testQuery.FilterString);
        }

        [Fact]
        public void TestGtFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField > 1));
            Assert.Equal("FloatField gt 1.0", testQuery.FilterString);
        }

        [Fact]
        public void TestLeFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField <= 1));
            Assert.Equal("FloatField le 1.0", testQuery.FilterString);
        }

        [Fact]
        public void TestGeFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField >= 1));
            Assert.Equal("FloatField ge 1.0", testQuery.FilterString);
        }

        [Fact]
        public void TestEqualFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField == 1));
            Assert.Equal("FloatField eq 1.0", testQuery.FilterString);
        }

        [Fact]
        public void TestNotEqualFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField != 1));
            Assert.Equal("FloatField ne 1.0", testQuery.FilterString);
        }

        [Fact]
        public void TestGeAndFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField >= 1 && e.FloatField <= 2));
            Assert.Equal("(FloatField ge 1.0) and (FloatField le 2.0)", testQuery.FilterString);
        }

        [Fact]
        public void TestGAndLFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField > 1 && e.FloatField < 2));
            Assert.Equal("(FloatField gt 1.0) and (FloatField lt 2.0)", testQuery.FilterString);
        }

        [Fact]
        public void TestEAndNeFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField == 1 && e.FloatField != 2));
            Assert.Equal("(FloatField eq 1.0) and (FloatField ne 2.0)", testQuery.FilterString);
        }

        [Fact]
        public void TestGeOrLeFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField >= 1 || e.FloatField <= 2));
            Assert.Equal("(FloatField ge 1.0) or (FloatField le 2.0)", testQuery.FilterString);
        }

        [Fact]
        public void TestGOrLFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField > 1 || e.FloatField < 2));
            Assert.Equal("(FloatField gt 1.0) or (FloatField lt 2.0)", testQuery.FilterString);
        }

        [Fact]
        public void TestEOrNeFloatField()
        {
            var testQuery = baseQuery.Where(e => (e.FloatField == 1 || e.FloatField != 2));
            Assert.Equal("(FloatField eq 1.0) or (FloatField ne 2.0)", testQuery.FilterString);
        }

        /// <summary>
        /// IntField
        /// </summary>
        [Fact]
        public void TestLtIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField < 1));
            Assert.Equal("IntField lt 1", testQuery.FilterString);
        }

        [Fact]
        public void TestGtIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField > 1));
            Assert.Equal("IntField gt 1", testQuery.FilterString);
        }

        [Fact]
        public void TestLeIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField <= 1));
            Assert.Equal("IntField le 1", testQuery.FilterString);
        }

        [Fact]
        public void TestGeIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField >= 1));
            Assert.Equal("IntField ge 1", testQuery.FilterString);
        }

        [Fact]
        public void TestEqualIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField == 1));
            Assert.Equal("IntField eq 1", testQuery.FilterString);
        }

        [Fact]
        public void TestNotEqualIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField != 1));
            Assert.Equal("IntField ne 1", testQuery.FilterString);
        }

        [Fact]
        public void TestGeAndIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField >= 1 && e.IntField <= 2));
            Assert.Equal("(IntField ge 1) and (IntField le 2)", testQuery.FilterString);
        }

        [Fact]
        public void TestGAndLIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField > 1 && e.IntField < 2));
            Assert.Equal("(IntField gt 1) and (IntField lt 2)", testQuery.FilterString);
        }

        [Fact]
        public void TestEAndNeIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField == 1 && e.IntField != 2));
            Assert.Equal("(IntField eq 1) and (IntField ne 2)", testQuery.FilterString);
        }

        [Fact]
        public void TestGeOrLeIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField >= 1 || e.IntField <= 2));
            Assert.Equal("(IntField ge 1) or (IntField le 2)", testQuery.FilterString);
        }

        [Fact]
        public void TestGOrLIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField > 1 || e.IntField < 2));
            Assert.Equal("(IntField gt 1) or (IntField lt 2)", testQuery.FilterString);
        }

        [Fact]
        public void TestEOrNeIntField()
        {
            var testQuery = baseQuery.Where(e => (e.IntField == 1 || e.IntField != 2));
            Assert.Equal("(IntField eq 1) or (IntField ne 2)", testQuery.FilterString);
        }

        /// <summary>
        /// PartKey
        /// </summary>
        [Fact]
        public void TestEqPartKey()
        {
            var testQuery = baseQuery.Where(e => (e.PartitionKey == "TTCV"));
            Assert.Equal("PartitionKey eq 'TTCV'", testQuery.FilterString);
        }

        [Fact]
        public void TestNePartKey()
        {
            var testQuery = baseQuery.Where(e => (e.PartitionKey != "TTCV"));
            Assert.Equal("PartitionKey ne 'TTCV'", testQuery.FilterString);
        }

        [Fact]
        public void TestEqAndNePartKey()
        {
            var testQuery = baseQuery.Where(e => (e.PartitionKey == "TTCV" && e.PartitionKey != "TTCV"));
            Assert.Equal("(PartitionKey eq 'TTCV') and (PartitionKey ne 'TTCV')", testQuery.FilterString);
        }

        [Fact]
        public void TestEqOrNePartKey()
        {
            var testQuery = baseQuery.Where(e => (e.PartitionKey == "TTCV" || e.PartitionKey != "TTCV"));
            Assert.Equal("(PartitionKey eq 'TTCV') or (PartitionKey ne 'TTCV')", testQuery.FilterString);
        }

        /// <summary>
        /// RKey
        /// </summary>
        [Fact]
        public void TestEqRKey()
        {
            var testQuery = baseQuery.Where(e => (e.RKey == "TTCV"));
            Assert.Equal("RowKey eq 'TTCV'", testQuery.FilterString);
        }

        [Fact]
        public void TestNeRKey()
        {
            var testQuery = baseQuery.Where(e => (e.RKey != "TTCV"));
            Assert.Equal("RowKey ne 'TTCV'", testQuery.FilterString);
        }

        [Fact]
        public void TestEqAndNeRKey()
        {
            var testQuery = baseQuery.Where(e => (e.RKey == "TTCV" && e.RKey != "TTCV"));
            Assert.Equal("(RowKey eq 'TTCV') and (RowKey ne 'TTCV')", testQuery.FilterString);
        }

        [Fact]
        public void TestEqOrNeRKey()
        {
            var testQuery = baseQuery.Where(e => (e.RKey == "TTCV" || e.RKey != "TTCV"));
            Assert.Equal("(RowKey eq 'TTCV') or (RowKey ne 'TTCV')", testQuery.FilterString);
        }

        /// <summary>
        /// StringField
        /// </summary>
        [Fact]
        public void TestSimpleString()
        {
            var testQuery = baseQuery.Where(e => e.StringField == "TTCV");
            Assert.Equal("StringField eq 'TTCV'", testQuery.FilterString);
        }

        public void TestNeStringField()
        {
            var testQuery = baseQuery.Where(e => (e.StringField != "TTCV"));
            Assert.Equal("StringField ne 'TTCV''", testQuery.FilterString);
        }

        [Fact]
        public void TestEqAndNeStringField()
        {
            var testQuery = baseQuery.Where(e => (e.StringField == "TTCV" && e.StringField != "TTCV"));
            Assert.Equal("(StringField eq 'TTCV') and (StringField ne 'TTCV')", testQuery.FilterString);
        }

        [Fact]
        public void TestEqOrNeStringField()
        {
            var testQuery = baseQuery.Where(e => (e.StringField == "TTCV" || e.StringField != "TTCV"));
            Assert.Equal("(StringField eq 'TTCV') or (StringField ne 'TTCV')", testQuery.FilterString);
        }
    }
}