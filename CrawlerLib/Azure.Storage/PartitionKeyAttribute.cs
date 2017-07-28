// <copyright file="PartitionKeyAttribute.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// Marks property used for PartitionKey
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PartitionKeyAttribute : Attribute
    {
    }
}