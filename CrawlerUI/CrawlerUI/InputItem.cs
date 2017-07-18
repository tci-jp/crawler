// <copyright file="InputItem.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    public class InputItem : INotifyPropertyChanged
    {
        private string url;
        private int depth;
        private int hostDepth;

        public string Url
        {
            get => url;
            set
            {
                if (value == url)
                {
                    return;
                }

                url = value;
                OnPropertyChanged();
            }
        }

        public int Depth
        {
            get => depth;
            set
            {
                if (value == depth)
                {
                    return;
                }

                depth = value;
                OnPropertyChanged();
            }
        }

        public int HostDepth
        {
            get => hostDepth;
            set
            {
                if (value == hostDepth)
                {
                    return;
                }

                hostDepth = value;
                OnPropertyChanged();
            }
        }

        public InputItem(string url, int depth, int hostDepth)
        {
            Url = url;
            Depth = depth;
            HostDepth = hostDepth;
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
