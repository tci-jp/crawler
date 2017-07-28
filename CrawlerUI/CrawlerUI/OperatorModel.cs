namespace CrawlerUI
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using CrawlerLib;
    using JetBrains.Annotations;

    public class OperatorModel : INotifyPropertyChanged
    {
        private SearchCondition.Operator op;
        private string value;

        public OperatorModel(string metadataName)
        {
            MetadataName = metadataName;
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public string Id { get; } = Guid.NewGuid().ToString();

        public string MetadataName { get; }

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

        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Equals(OperatorModel other)
        {
            return string.Equals(Id, other.Id);
        }
    }
}