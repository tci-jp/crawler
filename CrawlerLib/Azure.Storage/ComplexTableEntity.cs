// <copyright file="ComplexTableEntity.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace Azure.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <inheritdoc />
    /// <summary>
    /// Extension of <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity" /> with fields JSON serialization.
    /// </summary>
    public class ComplexTableEntity : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexTableEntity" /> class.
        /// </summary>
        [UsedImplicitly]
        protected ComplexTableEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexTableEntity" /> class.
        /// </summary>
        /// <param name="partitionKey">Partition Key.</param>
        /// <param name="rowKey">Row Key.</param>
        protected ComplexTableEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        /// <inheritdoc />
        public override void ReadEntity(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            foreach (var prop in GetType()
                .GetRuntimeProperties().Where(p => p.CanWrite))
            {
                if (properties.TryGetValue(prop.Name, out var entity))
                {
                    switch (entity.PropertyType)
                    {
                        case EdmType.Binary:
                            prop.SetValue(this, entity.BinaryValue);
                            break;
                        case EdmType.String:
                            prop.SetValue(this, entity.StringValue);
                            break;
                        case EdmType.Boolean:
                            prop.SetValue(this, entity.BooleanValue);
                            break;
                        case EdmType.DateTime:
                            prop.SetValue(this, entity.DateTime);
                            break;
                        case EdmType.Double:
                            prop.SetValue(this, entity.DoubleValue);
                            break;
                        case EdmType.Guid:
                            prop.SetValue(this, entity.GuidValue);
                            break;
                        case EdmType.Int32:
                            prop.SetValue(this, entity.Int32Value);
                            break;
                        case EdmType.Int64:
                            prop.SetValue(this, entity.Int64Value);
                            break;
                        default:
                            prop.SetValue(this, JsonConvert.DeserializeObject(entity.StringValue, prop.PropertyType));
                            break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var result = new Dictionary<string, EntityProperty>();
            foreach (var prop in GetType()
                .GetRuntimeProperties().Where(p => p.CanWrite))
            {
                var name = prop.Name;
                var val = prop.GetValue(this);
                switch (val)
                {
                    case string str:
                        result.Add(name, EntityProperty.GeneratePropertyForString(str));
                        break;
                    case bool bl:
                        result.Add(name, EntityProperty.GeneratePropertyForBool(bl));
                        break;
                    case DateTimeOffset dt:
                        result.Add(name, EntityProperty.GeneratePropertyForDateTimeOffset(dt));
                        break;
                    case DateTime dt:
                        result.Add(name, EntityProperty.GeneratePropertyForDateTimeOffset(new DateTimeOffset(dt)));
                        break;
                    case float db:
                        result.Add(name, EntityProperty.GeneratePropertyForDouble(db));
                        break;
                    case double db:
                        result.Add(name, EntityProperty.GeneratePropertyForDouble(db));
                        break;
                    case Guid gd:
                        result.Add(name, EntityProperty.GeneratePropertyForGuid(gd));
                        break;
                    case int it:
                        result.Add(name, EntityProperty.GeneratePropertyForInt(it));
                        break;
                    case short it:
                        result.Add(name, EntityProperty.GeneratePropertyForInt(it));
                        break;
                    case long lg:
                        result.Add(name, EntityProperty.GeneratePropertyForLong(lg));
                        break;
                    default:
                        result.Add(name, EntityProperty.GeneratePropertyForString(JsonConvert.SerializeObject(val)));
                        break;
                }
            }

            return result;
        }
    }
}