namespace WpfApp2
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using JetBrains.Annotations;

    public class Model : INotifyPropertyChanged
    {
        private int defaultDepth = 3;
        private int defaultHostDepth;

        private readonly StringBuilder log = new StringBuilder("Test\r\nTest");
        private string newUri;
        private bool running;
        private string currentContentUri;
        private string currentContent="";
        private string searchString;

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
        public ObservableCollection<string> Found { get; } = new ObservableCollection<string>();

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

        public string CurrentContentUri
        {
            get => currentContentUri; set
            {
                if (value == currentContentUri) return;
                currentContentUri = value;
                OnPropertyChanged();
            }
        }

        public string CurrentContent
        {
            get => currentContent;
            set
            {
                if (value == currentContent) return;
                currentContent = value;
                OnPropertyChanged();
            }
        }

        public string SearchString
        {
            get => searchString;
            set
            {
                if (value == searchString) return;
                searchString = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void AddLogLine(string line)
        {
            log.AppendLine(line);
            OnPropertyChanged(nameof(Log));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ResetLog()
        {
            log.Clear();
        }
    }
}