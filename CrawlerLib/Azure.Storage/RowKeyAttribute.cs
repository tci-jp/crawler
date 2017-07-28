// <copyright file="RowKeyAttribute.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// Marks property used for RowKey
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RowKeyAttribute : Attribute
    {
    }
}