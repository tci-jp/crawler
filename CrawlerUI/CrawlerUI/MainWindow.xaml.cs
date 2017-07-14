namespace WpfApp2
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using CrawlerLib;
    using CrawlerLib.Data;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex NumberRegex = new Regex("[^0-9]+");

        private static readonly DummyStorage storage = new DummyStorage();

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
                Model.NewUri = "";
            }
            else
            {
                MessageBox.Show("Not a URI.");
            }
        }

        private async void ContentUri_Selected(object sender, RoutedEventArgs e)
        {
            if (Model.CurrentContentUri != null)
            {
                var contentStream = await storage.GetUriContet(Model.CurrentContentUri);
                var reader = new StreamReader(contentStream);
                var content = await reader.ReadToEndAsync();
                Model.CurrentContent = content;
            }
            else
            {
                Model.CurrentContent = "";
            }
        }

        private void Crawler_UriCrawled(string uri)
        {
            Dispatcher.Invoke(() => { Model.Found.Add(uri); });
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
                Model.Found.Clear();
                var config = new Configuration
                {
                    CancellationToken = Model.Cancellation.Token,
                    Depth = Model.DefaultDepth,
                    HostDepth = Model.DefaultHostDepth,
                    Logger = new GuiLogger(Model),
                    Storage = storage
                };

                var crawler = new Crawler(config);
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
            Model.Found.Clear();
            var cancellation = Model.Cancellation.Token;
            string searchString = Model.SearchString;
            var task = Task.Run(async () =>
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
                              Model.Found.Add(uri);
                          });
                      }
                  }
                  catch (Exception ex)
                  {
                      Model.AddLogLine(ex.ToString());
                  }
              });
        }

        private void SearchText_Changed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }
    }
}