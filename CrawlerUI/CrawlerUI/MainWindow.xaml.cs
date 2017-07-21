// <copyright file="MainWindow.xaml.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI
{
    using System;
    using System.Collections.Async;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Azure.Storage;
    using CrawlerLib;
    using CrawlerLib.Azure;
    using CrawlerLib.Data;
    using CrawlerLib.Grabbers;

    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly DataStorage Azure = new DataStorage("UseDevelopmentStorage=true");
        private static readonly Regex NumberRegex = new Regex("[^0-9]+");

        private static readonly ICrawlerStorage Storage =
            new AzureCrawlerStorage(Azure, new SimpleBlobSearcher(Azure, "pages"));

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private Model Model => DataContext as Model;

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddUrl();
        }

        private void AddUrl()
        {
            var modelNewUri = Model.NewUri.Trim();
            if (Uri.TryCreate(modelNewUri, UriKind.Absolute, out _))
            {
                Model.Input.Add(new InputItem(modelNewUri, Model.DefaultDepth, Model.DefaultHostDepth));
                Model.NewUri = string.Empty;
            }
            else
            {
                MessageBox.Show("Not a URI.");
            }
        }

        private void Crawler_UriCrawled(Crawler crawler, string uri)
        {
            Dispatcher.Invoke(() => { Model.CrawlerResult.Add(uri); });
        }

        private async void CrawlerUri_Selected(object sender, RoutedEventArgs e)
        {
            if (Model.CurrentCrawlerUri != null)
            {
                var mem = new MemoryStream();
                await Storage.GetUriContet(Model.CurrentCrawlerUri, mem, CancellationToken.None);
                mem.Position = 0;
                var reader = new StreamReader(mem);
                var content = await reader.ReadToEndAsync();
                Model.CurrentCrawlerContent = content;
            }
            else
            {
                Model.CurrentCrawlerContent = string.Empty;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = NumberRegex.IsMatch(e.Text);
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            var urlItem = ((FrameworkElement)sender).DataContext as InputItem;
            Model.Input.Remove(urlItem);
        }

        private void Search()
        {
            Model.SearchCancellation?.Cancel();
            Model.SearchCancellation?.Dispose();
            Model.SearchCancellation = new CancellationTokenSource();
            Model.SearchResult.Clear();
            var cancellation = Model.SearchCancellation.Token;
            var searchString = Model.SearchString;
            var task = Task.Run(
                async () =>
                {
                    try
                    {
                        var en = await Storage.SearchText(searchString);
                        await en.ForEachAsync(
                            uri =>
                            {
                                cancellation.ThrowIfCancellationRequested();
                                Dispatcher.InvokeAsync(() => { Model.SearchResult.Add(uri); });
                            },
                            cancellation);
                    }
                    catch (Exception ex)
                    {
                        Model.AddLogLine(ex.ToString());
                    }
                },
                cancellation);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void SearchContent_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void SearchText_Changed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private async void SearchUri_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (Model.CurrentSearchUri != null)
            {
                var mem = new MemoryStream();
                await Storage.GetUriContet(Model.CurrentSearchUri, mem, CancellationToken.None);
                mem.Position = 0;
                var reader = new StreamReader(mem);
                var content = await reader.ReadToEndAsync();
                Model.CurrentSearchContent = content;
                var selStart =
                    content.IndexOf(Model.SearchString, 0, StringComparison.InvariantCultureIgnoreCase);

                FocusManager.SetFocusedElement(this, SearchContent);
                SearchContent.Select(selStart, Model.SearchString.Length);
            }
            else
            {
                Model.CurrentSearchContent = string.Empty;
            }
        }

        private async void StartCrawl(object sender, RoutedEventArgs e)
        {
            if (Model.IsCrawlerRunning)
            {
                return;
            }

            Model.IsCrawlerRunning = true;
            try
            {
                Model.CrawlerCancellation = new CancellationTokenSource();
                Model.CrawlerResult.Clear();
                var config = new Configuration
                             {
                                 CancellationToken = Model.CrawlerCancellation.Token,
                                 Depth = Model.DefaultDepth,
                                 HostDepth = Model.DefaultHostDepth,
                                 Logger = new GuiLogger(Model),
                                 Storage = Storage
                             };

                config.HttpGrabber = new WebDriverHttpGrabber(config);

                var crawler = new Crawler(config);
                crawler.UriCrawled += Crawler_UriCrawled;
                try
                {
                    await crawler.Incite(Model.Input.Select(i => new Uri(i.Uri)).ToList());
                }
                catch (TaskCanceledException)
                {
                    // Ignore
                }
                finally
                {
                    crawler.UriCrawled -= Crawler_UriCrawled;
                }
            }
            finally
            {
                Model.CrawlerCancellation.Dispose();
                Model.CrawlerCancellation = null;
                Model.IsCrawlerRunning = false;
            }
        }

        private void StopCrawl(object sender, RoutedEventArgs e)
        {
            Model.CrawlerCancellation?.Cancel();
        }

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LogScrollViewer.ScrollToBottom();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddUrl();
            }
        }
    }
}