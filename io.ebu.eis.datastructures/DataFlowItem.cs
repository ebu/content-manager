using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace io.ebu.eis.datastructures
{
    public enum DataFlowPriority
    {
        High = 1,
        Medium = 2,
        Low = 3,
        Neglectable = 100,
        Unknown = 0
    }
    public class DataFlowItem : INotifyPropertyChanged
    {
        private DateTime _timestamp;
        public DateTime Timestamp { get { return _timestamp; } set { _timestamp = value; OnPropertyChanged("Timestamp"); } }

        private string _type;
        public string Type { get { return _type; } set { _type = value; OnPropertyChanged("Type"); } }

        private string _cateogry;
        public string Category { get { return _cateogry; } set { _cateogry = value; OnPropertyChanged("Category"); } }

        private string _name;
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }

        private string _short;
        public string Short { get { return _short; } set { _short = value; OnPropertyChanged("Short"); } }

        private DataFlowPriority _priority;
        public DataFlowPriority Priority { get { return _priority; } set { _priority = value; OnPropertyChanged("Priority"); } }

        private BitmapImage _image;
        public BitmapImage Image { get { return _image; } set { _image = value; OnPropertyChanged("Image"); } }
        public double ImageHeight { get { return Image.Height; } }
        public double ImageWidth { get { return Image.Width; } }
        public ImageSource ImageSource { get { return Image; } }

        public override string ToString()
        {
            return Name;
        }


        public void LoadImageFromUrl(string url)
        {
            Image = new BitmapImage();
            Image.BeginInit();
            Image.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
            Image.EndInit();
        }


        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
        #endregion PropertyChanged
    }
}
