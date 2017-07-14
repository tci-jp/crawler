namespace WpfApp2
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Input;
    using CrawlerLib;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex NumberRegex = new Regex("[^0-9]+");

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
            Model.Running = true;
            try
            {
                var config = new Configuration
                {
                    Depth = Model.DefaultDepth,
                    HostDepth = Model.DefaultHostDepth
                };

                var crawler = new Crawler(config);
                await crawler.Incite(Model.Input.Select(i => new Uri(i.Url)));
            }
            finally
            {
                Model.Running = false;
            }
        }

        private void StopCrawl(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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