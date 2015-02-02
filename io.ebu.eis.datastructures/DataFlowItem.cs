using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
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
        Green = 4,
        Neglectable = 100,
        Unknown = 0
    }
    [DataContract]
    public class DataFlowItem : INotifyPropertyChanged
    {
        private DataMessage _dataMessage;
        [DataMember(Name = "datamessage")]
        public DataMessage DataMessage { get { return _dataMessage; } set { _dataMessage = value; OnPropertyChanged("DataMessage"); } }

        private DateTime _timestamp;
        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get { return _timestamp; } set { _timestamp = value; OnPropertyChanged("Timestamp"); } }

        private string _type;
        [DataMember(Name = "type")]
        public string Type { get { return _type; } set { _type = value; OnPropertyChanged("Type"); } }

        private string _cateogry;
        [DataMember(Name = "category")]
        public string Category { get { return _cateogry; } set { _cateogry = value; OnPropertyChanged("Category"); } }

        private string _name;
        [DataMember(Name = "name")]
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }

        private string _short;
        [DataMember(Name = "short")]
        public string Short { get { return _short; } set { _short = value; OnPropertyChanged("Short"); } }

        private string _url;
        [DataMember(Name = "url")]
        public string Url { get { return _url; } set { _url = value; OnPropertyChanged("Url"); } }

        private DataFlowPriority _priority;
        [DataMember(Name = "priority")]
        public DataFlowPriority Priority { get { return _priority; } set { _priority = value; OnPropertyChanged("Priority"); } }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get
            {
                if (_image == null)
                {
                    LoadImageFromUrl(Url);
                }
                return _image;
            }
            set { _image = value; OnPropertyChanged("Image"); }
        }
        public double ImageHeight { get { return Image.Height; } }
        public double ImageWidth { get { return Image.Width; } }
        public ImageSource ImageSource { get { return Image; } }

        public override string ToString()
        {
            return Name;
        }


        private void LoadImageFromUrl(string url)
        {
            try
            {
                Image = new BitmapImage();
                Image.BeginInit();
                Image.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
                Image.DecodePixelWidth = 200;
                Image.EndInit();
            }
            catch (Exception)
            {
                // TODO handle exceptions !
            }
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
