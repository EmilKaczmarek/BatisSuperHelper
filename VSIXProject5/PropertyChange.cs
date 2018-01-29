using EnvDTE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5
{
    public class PropertyChange : INotifyPropertyChanged
    {
        private DateTime? typingStarts;
        public TextPoint LastStartPoint;
        public TextPoint LastEndPoint;

        public DateTime? TypingStarts
        {
            get { return typingStarts; }
            set
            {
                typingStarts = value;
                OnPropertyChanged("typingStarts");
            }
        }
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);

        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
