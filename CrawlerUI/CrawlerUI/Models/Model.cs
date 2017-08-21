// <copyright file="Model.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI.Models
{
    using System.Collections.Generic;
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
        private IList<KeyValuePair<string, string>> currentCrawlerMetadata;
        private string currentCrawlerUri;
        private string currentSearchContent;
        private IList<KeyValuePair<string, string>> currentSearchMetadata;
        private string currentSearchUri;
        private int defaultDepth = 3;
        private int defaultHostDepth;
        private bool isCrawlerRunning;
        private bool isSearching;
        private string newUri;
        private string searchString;

        /// <summary>
        /// Initializes a new instance of the <see cref="Model" /> class.
        /// </summary>
        public Model()
        {
            MetaConditions.CollectionChanged += (o, args) => OnPropertyChanged(nameof(IsTextSearch));
        }

        /// <summary>
        /// See <see cref="INotifyPropertyChanged" />
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets metadata available to search.
        /// </summary>
        public ObservableCollection<string> AvailableMetadata { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets cancellation Token Source to stop crawler.
        /// </summary>
        public CancellationTokenSource CrawlerCancellation { get; set; }

        /// <summary>
        /// Gets uRIs found by crawler including initial.
        /// </summary>
        public ObservableCollection<string> CrawlerResult { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets content of selected URI selected in the list of URIs found by Crawler.
        /// </summary>
        public string CurrentCrawlerContent
        {
            get => currentCrawlerContent;
            set
            {
                if (value != currentCrawlerContent)
                {
                    currentCrawlerContent = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets list of page metadata for selected URI in crawler tab.
        /// </summary>
        public IList<KeyValuePair<string, string>> CurrentCrawlerMetadata
        {
            get => currentCrawlerMetadata;
            set
            {
                if (!Equals(value, currentCrawlerMetadata))
                {
                    currentCrawlerMetadata = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets uRI selected in the list of URIs found by Crawler.
        /// </summary>
        public string CurrentCrawlerUri
        {
            get => currentCrawlerUri;
            set
            {
                if (value != currentCrawlerUri)
                {
                    currentCrawlerUri = value;
                    OnPropertyChanged();
                }
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
                if (value != currentSearchContent)
                {
                    currentSearchContent = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets list of metadata for current page selected on Search tab.
        /// </summary>
        public IList<KeyValuePair<string, string>> CurrentSearchMetadata
        {
            get => currentSearchMetadata;
            set
            {
                if (!Equals(value, currentSearchMetadata))
                {
                    currentSearchMetadata = value;
                    OnPropertyChanged();
                }
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
                if (value != currentSearchUri)
                {
                    currentSearchUri = value;
                    OnPropertyChanged();
                }
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
                if (value != defaultDepth)
                {
                    defaultDepth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets crawling Host Depth.
        /// </summary>
        public int DefaultHostDepth
        {
            get => defaultHostDepth;
            set
            {
                if (value != defaultHostDepth)
                {
                    defaultHostDepth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets initial Crawling URIs.
        /// </summary>
        public ObservableCollection<InputItem> Input { get; } = new ObservableCollection<InputItem>();

        /// <summary>
        /// Gets or sets a value indicating whether true if crawling.
        /// </summary>
        public bool IsCrawlerRunning
        {
            get => isCrawlerRunning;
            set
            {
                if (value != isCrawlerRunning)
                {
                    isCrawlerRunning = value;
                    OnPropertyChanged();
                }
            }
        }

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
        /// Gets a value indicating whether search is full text (true) or metadata query (false)
        /// </summary>
        public bool IsTextSearch => !MetaConditions.Any();

        /// <summary>
        /// Gets log content.
        /// </summary>
        public string Log => log.ToString();

        /// <summary>
        /// Gets current metadata search conditions.
        /// </summary>
        public ObservableCollection<OperatorModel> MetaConditions { get; } =
            new ObservableCollection<OperatorModel>();

        /// <summary>
        /// Gets or sets uRI to add.
        /// </summary>
        public string NewUri
        {
            get => newUri;
            set
            {
                if (value != newUri)
                {
                    newUri = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets cancellation Token Source to stop search.
        /// </summary>
        public CancellationTokenSource SearchCancellation { get; set; }

        /// <summary>
        /// Gets uRIs found by searching.
        /// </summary>
        public ObservableCollection<string> SearchResult { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets text in search line.
        /// </summary>
        public string SearchString
        {
            get => searchString;
            set
            {
                if (value != searchString)
                {
                    searchString = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets gets name of metadata selected for adding into query
        /// </summary>
        public string SelectedMetadata { get; set; }

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