// <copyright file="Model.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using JetBrains.Annotations;

    public class Model : INotifyPropertyChanged
    {
        private readonly StringBuilder log = new StringBuilder("Test\r\nTest");
        private string currentCrawlerContent = string.Empty;
        private string currentCrawlerUri;
        private string currentSearchContent;
        private string currentSearchUri;
        private int defaultDepth = 3;
        private int defaultHostDepth;
        private string newUri;
        private bool running;
        private string searchString;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        public string NewUri
        {
            get => newUri;
            set
            {
                if (value == newUri)
                {
                    return;
                }

                newUri = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<InputItem> Input { get; } = new ObservableCollection<InputItem>();

        public ObservableCollection<string> CrawlerResult { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> SearchResult { get; } = new ObservableCollection<string>();

        public int DefaultHostDepth
        {
            get => defaultHostDepth;
            set
            {
                if (value == defaultHostDepth)
                {
                    return;
                }

                defaultHostDepth = value;
                OnPropertyChanged();
            }
        }

        public int DefaultDepth
        {
            get => defaultDepth;
            set
            {
                if (value == defaultDepth)
                {
                    return;
                }

                defaultDepth = value;
                OnPropertyChanged();
            }
        }

        public bool Running
        {
            get => running;
            set
            {
                if (value == running)
                {
                    return;
                }

                running = value;
                OnPropertyChanged();
            }
        }

        public CancellationTokenSource Cancellation { get; set; }

        public string Log => log.ToString();

        public string CurrentCrawlerUri
        {
            get => currentCrawlerUri;
            set
            {
                if (value == currentCrawlerUri)
                {
                    return;
                }

                currentCrawlerUri = value;
                OnPropertyChanged();
            }
        }

        public string CurrentSearchUri
        {
            get => currentSearchUri;
            set
            {
                if (value == currentSearchUri)
                {
                    return;
                }

                currentSearchUri = value;
                OnPropertyChanged();
            }
        }

        public string CurrentSearchContent
        {
            get => currentSearchContent;
            set
            {
                if (value == currentSearchContent)
                {
                    return;
                }

                currentSearchContent = value;
                OnPropertyChanged();
            }
        }

        public string CurrentCrawlerContent
        {
            get => currentCrawlerContent;
            set
            {
                if (value == currentCrawlerContent)
                {
                    return;
                }

                currentCrawlerContent = value;
                OnPropertyChanged();
            }
        }

        public string SearchString
        {
            get => searchString;
            set
            {
                if (value == searchString)
                {
                    return;
                }

                searchString = value;
                OnPropertyChanged();
            }
        }

        public int SearchContentSelectionStart { get; set; }
        public int SearchContentSelectionLength { get; set; }

        public void AddLogLine(string line)
        {
            log.AppendLine(line);
            OnPropertyChanged(nameof(Log));
        }

        public void ResetLog()
        {
            log.Clear();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}