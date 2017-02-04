using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using io.ebu.eis.canvasgenerator;
using io.ebu.eis.data.file;
using io.ebu.eis.data.ftp;
using io.ebu.eis.data.s3;
using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;
using io.ebu.eis.stomp;

namespace io.ebu.eis.shared
{
    [DataContract]
    public class WorkerTaskMessage : INotifyPropertyChanged
    {
        private string _id;
        [DataMember(Name = "id")]
        public string Id { get { return _id; } set { _id = value; OnPropertyChanged("Id"); } }

        private long _serial;
        [DataMember(Name = "serial")]
        public long Serial { get { return _serial; } set { _serial = value; OnPropertyChanged("Serial"); } }

        private string _type;
        [DataMember(Name = "type")]
        public string Type { get { return _type; } set { _type = value; OnPropertyChanged("Type"); } }

        private string _generationProps;
        [DataMember(Name = "generationprops")]
        public string GenerationProps { get { return _generationProps; } set { _generationProps = value; OnPropertyChanged("GenerationProps"); } }

        private string _base64ImageData;
        [DataMember(Name = "base64imagedata", IsRequired = false)]
        public string Base64ImageData { get { return _base64ImageData; } set { _base64ImageData = value; OnPropertyChanged("Base64ImageData"); } }

        private string _imageFormat;
        [DataMember(Name = "imageformat", IsRequired = false)]
        public string ImageFormat { get { return _imageFormat; } set { _imageFormat = value; OnPropertyChanged("ImageFormat"); } }

        private int _imageWidth;
        [DataMember(Name = "imagewidth", IsRequired = false)]
        public int ImageWidth { get { return _imageWidth; } set { _imageWidth = value; OnPropertyChanged("ImageWidth"); } }

        private int _imageHeight;
        [DataMember(Name = "imageheight", IsRequired = false)]
        public int ImageHeight { get { return _imageHeight; } set { _imageHeight = value; OnPropertyChanged("ImageHeight"); } }

        private ManagerImageReference _imageReference;
        [DataMember(Name = "imagereference", IsRequired = false)]
        public ManagerImageReference ImageReference { get { return _imageReference; } set { _imageReference = value; OnPropertyChanged("ImageReference"); } }
        
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
