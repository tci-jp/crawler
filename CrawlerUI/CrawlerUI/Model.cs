// <copyright file="Model.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// The main View Model.
    /// </summary>
    public class Model : INotifyPropertyChanged
    {
        private readonly StringBuilder log = new StringBuilder();
        private string currentCrawlerContent = string.Empty;
        private string currentCrawlerUri;
        private string currentSearchContent;
        private string currentSearchUri;
        private int defaultDepth = 3;
        private int defaultHostDepth;
        private bool isCrawlerRunning;
        private bool isSearching;
        private string newUri;
        private string searchString;

        /// <summary>
        /// See <see cref="INotifyPropertyChanged" />
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets uRI to add.
        /// </summary>
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

        /// <summary>
        /// Gets initial Crawling URIs.
        /// </summary>
        public ObservableCollection<InputItem> Input { get; } = new ObservableCollection<InputItem>();

        /// <summary>
        /// Gets uRIs found by crawler including initial.
        /// </summary>
        public ObservableCollection<string> CrawlerResult { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets uRIs found by searching.
        /// </summary>
        public ObservableCollection<string> SearchResult { get; } = new ObservableCollection<string>();

        public Model()
        {
            MetaConditions.CollectionChanged += (o, args) => OnPropertyChanged(nameof(IsTextSearch));
        }

        /// <summary>
        /// Gets or sets crawling Host Depth.
        /// </summary>
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

        /// <summary>
        /// Gets or sets crawler Depth.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether true if crawling.
        /// </summary>
        public bool IsCrawlerRunning
        {
            get => isCrawlerRunning;
            set
            {
                if (value == isCrawlerRunning)
                {
                    return;
                }

                isCrawlerRunning = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets cancellation Token Source to stop crawler.
        /// </summary>
        public CancellationTokenSource CrawlerCancellation { get; set; }

        /// <summary>
        /// Gets log content.
        /// </summary>
        public string Log => log.ToString();

        /// <summary>
        /// Gets or sets uRI selected in the list of URIs found by Crawler.
        /// </summary>
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

        /// <summary>
        /// Gets or sets uRI selected in the list of URIs found by Search.
        /// </summary>
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

        /// <summary>
        /// Gets or sets content of selected URI selected in the list of URIs found by Search.
        /// </summary>
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

        /// <summary>
        /// Gets or sets content of selected URI selected in the list of URIs found by Crawler.
        /// </summary>
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

        /// <summary>
        /// Gets or sets text in search line.
        /// </summary>
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

        /// <summary>
        /// Gets or sets cancellation Token Source to stop search.
        /// </summary>
        public CancellationTokenSource SearchCancellation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether search is in process.
        /// </summary>
        public bool IsSearching
        {
            get => isSearching;
            set
            {
                if (value == isSearching)
                {
                    return;
                }

                isSearching = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets metadata available to search.
        /// </summary>
        public ObservableCollection<string> AvailableMetadata { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets current metadata search conditions.
        /// </summary>
        public ObservableCollection<OperatorModel> MetaConditions { get; } =
            new ObservableCollection<OperatorModel>();

        public string SelectedMetadata { get; set; }

        public bool IsTextSearch => !MetaConditions.Any();

        /// <summary>
        /// Add line into log.
        /// </summary>
        /// <param name="line">Line to add.</param>
        public void AddLogLine(string line)
        {
            log.AppendLine(line);
            OnPropertyChanged(nameof(Log));
        }

        /// <summary>
        /// Reset Log content.
        /// </summary>
        public void ResetLog()
        {
            log.Clear();
        }

        /// <summary>
        /// See <see cref="INotifyPropertyChanged" />
        /// </summary>
        /// <param name="propertyName">Name of property to notify.</param>
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}