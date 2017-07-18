// <copyright file="MainWindow.xaml.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerUI
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using CrawlerLib;
    using CrawlerLib.Data;
    using CrawlerLib.Grabbers;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex NumberRegex = new Regex("[^0-9]+");

        private static readonly DummyStorage storage = new DummyStorage();
        private HttpGrabber grabber;

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

        private async void CrawlerUri_Selected(object sender, RoutedEventArgs e)
        {
            if (Model.CurrentCrawlerUri != null)
            {
                var contentStream = await storage.GetUriContet(Model.CurrentCrawlerUri);
                var reader = new StreamReader(contentStream);
                var content = await reader.ReadToEndAsync();
                Model.CurrentCrawlerContent = content;
            }
            else
            {
                Model.CurrentCrawlerContent = string.Empty;
            }
        }

        private void Crawler_UriCrawled(string uri)
        {
            Dispatcher.Invoke(() => { Model.CrawlerResult.Add(uri); });
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

        private async void StartCrawl(object sender, RoutedEventArgs e)
        {
            if (Model.Running)
            {
                return;
            }

            Model.Running = true;
            try
            {
                Model.Cancellation = new CancellationTokenSource();
                Model.CrawlerResult.Clear();
                var config = new Configuration
                {
                    CancellationToken = Model.Cancellation.Token,
                    Depth = Model.DefaultDepth,
                    HostDepth = Model.DefaultHostDepth,
                    Logger = new GuiLogger(Model),
                    Storage = storage
                };

                if (grabber == null)
                {
                    grabber = new HttpContentGrabber(config);
                }

                var crawler = new Crawler(grabber, config);
                crawler.UriCrawled += Crawler_UriCrawled;
                try
                {
                    await crawler.Incite(Model.Input.Select(i => new Uri(i.Url)).ToList());
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
                Model.Cancellation.Dispose();
                Model.Cancellation = null;
                Model.Running = false;
            }
        }

        private void StopCrawl(object sender, RoutedEventArgs e)
        {
            Model.Cancellation?.Cancel();
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

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void Search()
        {
            Model.Cancellation?.Cancel();
            Model.Cancellation?.Dispose();
            Model.Cancellation = new CancellationTokenSource();
            Model.SearchResult.Clear();
            var cancellation = Model.Cancellation.Token;
            var searchString = Model.SearchString;
            var task = Task.Run(
                async () =>
                  {
                      try
                      {
                          var en = await storage.SearchText(searchString);
                          foreach (var uri in en)
                          {
                              if (cancellation.IsCancellationRequested)
                              {
                                  break;
                              }
                              Dispatcher.Invoke(() =>
                              {
                                  Model.SearchResult.Add(uri);
                              });
                          }
                      }
                      catch (Exception ex)
                      {
                          Model.AddLogLine(ex.ToString());
                      }
                  },
                cancellation);
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
                var contentStream = await storage.GetUriContet(Model.CurrentSearchUri);
                var reader = new StreamReader(contentStream);
                var content = await reader.ReadToEndAsync();
                Model.CurrentSearchContent = content;
                Model.SearchContentSelectionStart =
                    content.IndexOf(Model.SearchString, 0, StringComparison.InvariantCultureIgnoreCase);
                Model.SearchContentSelectionLength = Model.SearchString.Length;

                FocusManager.SetFocusedElement(this, SearchContent);
                SearchContent.Select(Model.SearchContentSelectionStart, Model.SearchContentSelectionLength);
            }
            else
            {
                Model.CurrentSearchContent = string.Empty;
            }
        }

        private void SearchContent_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}