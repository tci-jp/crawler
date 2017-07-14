namespace WpfApp2
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;

    public class Model : INotifyPropertyChanged
    {
        private string newUri;
        private int defaultHostDepth = 0;
        private int defaultDepth = 3;
        private bool running;

        public string NewUri
        {
            get => newUri;
            set
            {
                if (value == newUri) return;
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
                if (value == defaultHostDepth) return;
                defaultHostDepth = value;
                OnPropertyChanged();
            }
        }

        public int DefaultDepth
        {
            get => defaultDepth;
            set
            {
                if (value == defaultDepth) return;
                defaultDepth = value;
                OnPropertyChanged();
            }
        }

        public bool Running
        {
            get => running; set
            {
                if (value == running) return;
                running = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}