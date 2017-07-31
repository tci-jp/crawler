// <copyright file="OperatorModel.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI.Models
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using CrawlerLib;
    using CrawlerLib.Data;
    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// Model for metadata query search item.
    /// </summary>
    public sealed class OperatorModel : INotifyPropertyChanged
    {
        private SearchCondition.Operator op;
        private string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorModel"/> class.
        /// </summary>
        /// <param name="metadataName">Metadata field name.</param>
        public OperatorModel(string metadataName)
        {
            MetadataName = metadataName;
        }

        /// <summary>
        /// See <see cref="INotifyPropertyChanged"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets id for comparison.
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets metadata field name.
        /// </summary>
        public string MetadataName { get; }

        /// <summary>
        /// Gets or sets query field comparison operator.
        /// </summary>
        public SearchCondition.Operator Op
        {
            get => op;
            set
            {
                if (value == op)
                {
                    return;
                }

                op = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets query field comparison value.
        /// </summary>
        public string Value
        {
            get => value;
            set
            {
                if (value == this.value)
                {
                    return;
                }

                this.value = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((OperatorModel)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool Equals(OperatorModel other)
        {
            return string.Equals(Id, other.Id);
        }
    }
}