using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using CrawlerLib;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.Collections.Concurrent;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int depth;
        public int hostDepth;
        public string searchKeyword;
        public string subUrlsString;
        private Crawler crawler;
        private DummyStorage storage;
        //static public ObservableHashSet<String> Links;
        static public ObservableHashSet<String> Links;
        static private ObservableHashSet<Item> UrlItems;
        static public Dictionary<string, CrawlerUI.TasksForEachUrl> TaskDict;

        //static private ObservableCollection<Item> Links;
        //private HashSet<Item> ModifiedItems { get; set; }

        static public HashSet<string> urls = new HashSet<string>();
        private ICollectionView view;

        private const double INTERVAL = 0.3;
        private DispatcherTimer timer;
        private int finalDumpedPagesNumber;

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            crawler = new Crawler();
            storage = crawler.Config.Storage as DummyStorage;

            UrlItems = new ObservableHashSet<Item>() { };
            Links = new ObservableHashSet<string>() { };

            //Links = new ObservableCollection<HashSet<string>>();
            //Links.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
            UrlDataGrid.ItemsSource = UrlItems;
            SubUrlListView.ItemsSource = Links;

            //ModifiedItems = new HashSet<Item>();
            //Links = new ObservableCollection<Item>();          
            //Links.CollectionChanged += this.OnCollectionChanged;

            //Links.Add(new Item("test Item"));

            view = CollectionViewSource.GetDefaultView(Links);
            view.Refresh();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(INTERVAL)
            };
            timer.Tick += (o, e) =>
            {
                try
                {
                    view.Refresh();

                }catch(Exception exc)
                {
                    Console.WriteLine("Error when refresh view : " + exc);
                }
                
            };
            timer.IsEnabled = true;

            DataContext = this;
        }

        private void StartCrawl(object sender, RoutedEventArgs e)
        {
            try
            {
                depth = int.Parse(DepthTextBox.Text);
                hostDepth = int.Parse(HostDepthTextBox.Text);
            }
            catch (FormatException fe)
            {
                Console.WriteLine(fe);
            }
            
            string url = UrlTextBox0.Text;
            TaskDict = new Dictionary<string, CrawlerUI.TasksForEachUrl>();
            finalDumpedPagesNumber = 0;
            crawler = new Crawler();
            storage = crawler.Config.Storage as DummyStorage;

            //ICollectionView view = CollectionViewSource.GetDefaultView(Links);
            //view = CollectionViewSource.GetDefaultView(Links);
            //view.Refresh();

            if (UrlItems.Count > 0)
            {                

                //Crawl
                foreach(var item in UrlItems)
                {
                    Console.WriteLine("Start crawling url : " + item.Url);

                    CrawlerUI.TasksForEachUrl newT = new CrawlerUI.TasksForEachUrl();

                    newT.Finished = false;
                    newT.CrawTask = Task.Run(() => Craw(item.Url, item.Depth, item.HostDepth));
                    newT.UpdateListTask = 
                        Task.Run(() =>
                        {
                            while (true)
                            {
                                if (storage.DumpedPagesNumber > Links.Count)
                                { 
                                    try
                                    {
                                        foreach (var link in storage.DumpedPages)
                                        {
                                            if (! Links.Contains(link.Uri)) Console.WriteLine(link.Uri);
                                            Dispatcher.Invoke(new Action(() =>
                                            {                                               
                                               Links.Add(link.Uri);                                                
                                            }));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Error when adding items into Links : " + ex);
                                    }

                                }
                                //TODO : this following condition is not correct : )
                                //exit when list is fully updated
                                if (Links.Count + UrlItems.Count -1  == finalDumpedPagesNumber && finalDumpedPagesNumber != 0)
                                {
                                    Console.Write("Links Count = " + storage.DumpedPagesNumber + " ");
                                    break;
                                }
                            }

                        });

                    TaskDict.Add(item.Url, newT);

                    Console.WriteLine("Finished crawling url : " + item.Url);
                }
                
                //stop refreshing
                timer.IsEnabled = false;

                //foreach (var link in Links)
                //{
                //    Console.WriteLine(link);
                //}
            }
        }

        public async Task<bool> Craw(string url, int depth, int hostDepth)
        {
            crawler.Config.HostDepth = hostDepth;
            crawler.Config.Depth = depth;

            //start crawling
            await crawler.Incite(new Uri(url));

            Console.WriteLine("crawl finished !");

            finalDumpedPagesNumber += storage.DumpedPagesNumber;

            if (TaskDict.TryGetValue(url, out CrawlerUI.TasksForEachUrl T)){
                T.Finished = true;
                T.Count = finalDumpedPagesNumber;
            }

            Console.WriteLine("DumpedPagesNumber : " + storage.DumpedPagesNumber.ToString());
            Console.WriteLine("Links : " + Links.Count);

            return true;
        }

        //class Item : INotifyPropertyChanged
        //{
        //    private string name;
        //    public event PropertyChangedEventHandler PropertyChanged;
        //    protected void OnPropertyChanged(string name)
        //    {
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        //    }
        //    public Item()
        //    {
        //    }

        //    public Item(string value)
        //    {
        //        this.name = value;
        //    }

        //    public string ItemName
        //    {
        //        get { return name; }
        //        set
        //        {
        //            name = value;
        //            // Call OnPropertyChanged whenever the property is updated
        //            OnPropertyChanged("ItemName");
        //        }
        //    }
        //}

        //void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems != null)
        //    {
        //        foreach (Item newItem in e.NewItems)
        //        {
        //            ModifiedItems.Add(newItem);

        //            //Add listener for each item on PropertyChanged event
        //            newItem.PropertyChanged += this.OnItemPropertyChanged;
        //        }
        //    }

        //    if (e.OldItems != null)
        //    {
        //        foreach (Item oldItem in e.OldItems)
        //        {
        //            ModifiedItems.Add(oldItem);

        //            oldItem.PropertyChanged -= this.OnItemPropertyChanged;
        //        }
        //    }
        //}

        //void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    Item item = sender as Item;
        //    if (item != null)
        //        ModifiedItems.Add(item);
        //}


        public class ObservableHashSet<Item> : ObservableCollection<Item>
        {
            protected override void InsertItem(int index, Item item)
            {
                if (Contains(item))
                {
                    //throw new ItemExistsException(item);                   
                }
                else
                {
                    base.InsertItem(index, item);
                }
                
            }

            protected override void SetItem(int index, Item item)
            {
                int i = IndexOf(item);
                if (i >= 0 && i != index)
                {
                    //throw new ItemExistsException(item);
                }
                else
                {
                    base.SetItem(index, item);
                }
                
            }
        }

        class Item
        {
            public string Url { get; set; }
            public int Depth { get; set; }
            public int HostDepth { get; set; }
            //public Button RemoveBtn { get; set; } 

            public Item()
            {
            }

            public Item(string url, int depth, int hostDepth)
            {
                Url = url;
                Depth = depth;
                HostDepth = hostDepth;
                //RemoveBtn = removeBtn;
            }
        } 

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Button newBtn = new Button() { Content = "X" };
            //newBtn.Click += new RoutedEventHandler(RemoveBtn_Click);
            UrlItems.Add(new Item(
                UrlTextBox0.Text, 
                int.Parse(DepthTextBox.Text), 
                int.Parse(HostDepthTextBox.Text)));            

            UrlTextBox0.Text = String.Empty;
            
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            Item urlItem = ((FrameworkElement)sender).DataContext as Item;
            UrlItems.Remove(urlItem);

        }

        //private void GridView_RowSelected(object sender, RoutedEventArgs e)
        //{

        //}
    }

}
