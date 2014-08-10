using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using io.ebu.eis.canvasgenerator;
using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using io.ebu.eis.mq;

namespace io.ebu.eis.contentmanager
{
    public class ManagerContext : INotifyPropertyChanged, IAMQDataMessageHandler
    {
        private CMConfigurationSection _config;
        private AMQConsumer _dataInConnection;
        private AMQQueuePublisher _dataOutConnection;

        private HTMLRenderer _renderer;
        public HTMLRenderer Renderer { get { return _renderer; } }

        private bool _inAutomationMode;
        public bool InAutomationMode { get { return _inAutomationMode; } set { _inAutomationMode = value; OnPropertyChanged("InAutomationMode"); } }

        private int _automationInterval = 15;
        public int AutomationInterval { get { return _automationInterval; } set { _automationInterval = value; OnPropertyChanged("AutomationInterval"); } }

        private double automationProgress = 0.0;
        public double AutomationProgress { get { return automationProgress; } set { automationProgress = value; OnPropertyChanged("AutomationProgress"); } }

        private ManagerCart _activeCart;
        public ManagerCart ActiveCart { get { return _activeCart; } set { _activeCart = value; OnPropertyChanged("ActiveCart"); } }


        private ManagerCart _editorCart;
        public ManagerCart EditorCart { get { return _editorCart; } set { _editorCart = value; OnPropertyChanged("EditorCart"); } }


        private DispatchedObservableCollection<ManagerCart> _carts;
        public DispatchedObservableCollection<ManagerCart> Carts { get { return _carts; } set { _carts = value; OnPropertyChanged("Carts"); } }

        #region Database
        private DispatchedObservableCollection<DataMessage> _dataBase;
        public DispatchedObservableCollection<DataMessage> DataBase { get { return _dataBase; } set { _dataBase = value; OnPropertyChanged("DataBase"); } }

        //private DispatchedObservableCollection<EventFlow> _runningEvents;
        //public DispatchedObservableCollection<EventFlow> RunningEvents { get { return _runningEvents; } set { _runningEvents = value; OnPropertyChanged("RunningEvents"); } }

        private DispatchedObservableCollection<DataFlowItem> _dataFlowItems;
        public DispatchedObservableCollection<DataFlowItem> DataFlowItems { get { return _dataFlowItems; } set { _dataFlowItems = value; OnPropertyChanged("DataFlowItems"); } }
        public ICollectionView DataFlowItemsView;

        private DispatchedObservableCollection<DataFlowItem> _imageFlowItems;
        public DispatchedObservableCollection<DataFlowItem> ImageFlowItems { get { return _imageFlowItems; } set { _imageFlowItems = value; OnPropertyChanged("ImageFlowItems"); } }
        public ICollectionView ImageFlowItemsView;
        #endregion Database

        public ManagerContext()
        {
            _config = (CMConfigurationSection)ConfigurationManager.GetSection("CMConfiguration");

            // Initialize HTML Renderer
            _renderer = new HTMLRenderer(_config.SlidesConfiguration.TemplatePath);

            Carts = new DispatchedObservableCollection<ManagerCart>();
            ActiveCart = new ManagerCart("INIT");

            DataBase = new DispatchedObservableCollection<DataMessage>();
            //RunningEvents = new DispatchedObservableCollection<EventFlow>();
            DataFlowItems = new DispatchedObservableCollection<DataFlowItem>();
            ImageFlowItems = new DispatchedObservableCollection<DataFlowItem>();

            DataFlowItemsView = CollectionViewSource.GetDefaultView(DataFlowItems);
            DataFlowItemsView.Filter = DataFlowNameFilter;
            var sortData = new SortDescription {Direction = ListSortDirection.Descending, PropertyName = "Timestamp"};
            DataFlowItemsView.SortDescriptions.Add(sortData);

            ImageFlowItemsView = CollectionViewSource.GetDefaultView(ImageFlowItems);
            ImageFlowItemsView.Filter = ImageFlowNameFilter;
            var sortImage = new SortDescription {Direction = ListSortDirection.Descending, PropertyName = "Timestamp"};
            ImageFlowItemsView.SortDescriptions.Add(sortImage);

            // Open Connection to INBOUND and OUTBOUND MQ
            var amquri = _config.MQConfiguration.Uri;
            var amqinexchange = _config.MQConfiguration.DPExchange;
            _dataInConnection = new AMQConsumer(amquri, amqinexchange, this);
            _dataInConnection.Connect();

            var amqoutexchange = _config.MQConfiguration.DDExchange;
            _dataOutConnection = new AMQQueuePublisher(amquri, amqoutexchange);
            _dataOutConnection.Connect();
            // TODO Catch hand handle connection exceptions and reconnect

            LoadCarts();
            LoadInitialImages();
            LoadReceivedImages();
        }

        public void Stop()
        {
            _dataInConnection.Disconnect();
            _dataOutConnection.Disconnect();
        }

        private void LoadCarts()
        {
            foreach (CartConfiguration cart in _config.SlidesConfiguration.CartConfigurations)
            {
                var c = new ManagerCart(cart.Name);
                Carts.Add(c);
                foreach (SlideConfiguration s in cart.Slides)
                {
                    var sl = new ManagerImageReference(Renderer, _config)
                    {
                        Template = s.Filename,
                        Link = s.DefaultLink,
                        CanRepeate = s.CanRepeat
                    };
                    c.Slides.Add(sl);
                }

                if (cart.Active)
                {
                    // Set current cart as active
                    ActiveCart = c;
                }
                if (cart.EditorDefault)
                {
                    // Set current editor cart
                    EditorCart = c;
                }
            }
        }

        public void LoadInitialImages()
        {
            ReloadLoadImages(_config.DataConfiguration.InitialPictureFolder);
        }

        public void LoadReceivedImages()
        {
            ReloadLoadImages(_config.DataConfiguration.IncomingPictureFolder);
        }

        private void ReloadLoadImages(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception)
                {
                    // TODO LOG
                    return;
                }
            }

            foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".jpg") || s.EndsWith(".png") || s.EndsWith(".gif") || s.EndsWith(".jpeg")))
            {
                IngestImage(file);
            }
        }

        public void IngestImage(string file)
        {
            try
            {
                var title = new DataMessage() {Key = "TITLE", Value = Path.GetFileName(file)};
                var url = new DataMessage() {Key = "URL", Value = file};
                var type = new DataMessage() {Key = "TYPE", Value = Path.GetExtension(file)};
                var im = new DataMessage()
                {
                    DataType = "IMAGE",
                    Key = Path.GetFileName(file),
                    Value = file
                };
                im.Data.Add(title);
                im.Data.Add(url);
                im.Data.Add(type);

                OnReceive(im);
            }
            catch (Exception) { }
        }

        public void SwitchToNextSlide()
        {
            if (ActiveCart != null && ActiveCart.Slides.Count > 0)
            {
                // We can switch to the next one.
                MainImage = ActiveCart.GetNextSlide();
                ReloadPreview();
            }
        }

        public void ReloadPreview()
        {
            PreviewImage = ActiveCart.PreviewNextSlide();
        }
        public void SwitchToSlide(ManagerImageReference newImage)
        {
            // We can switch to the next one.
            ActiveCart.SetAllSlidesInactive();
            MainImage = newImage;
            newImage.IsActive = true;
            ReloadPreview();
        }

        #region FilteringAndSorting

        /** DATAFLOW Filter **/
        private bool DataFlowNameFilter(object item)
        {
            DataFlowItem data = item as DataFlowItem;
            return data.Name.ToLower().Contains(_dataFlowFilterString.ToLower())
                || data.Category.ToLower().Contains(_dataFlowFilterString.ToLower())
                || data.Type.ToLower().Contains(_dataFlowFilterString.ToLower());
        }
        private string _dataFlowFilterString = "";
        public string DataFlowFilterString
        {
            get { return _dataFlowFilterString; }
            set
            {
                _dataFlowFilterString = value;
                OnPropertyChanged("DataFlowFilterString");
                DataFlowItemsView.Refresh();
            }
        }

        /** IMAGE Filter **/
        private bool ImageFlowNameFilter(object item)
        {
            DataFlowItem data = item as DataFlowItem;
            return data.Name.ToLower().Contains(_imageFlowFilterString.ToLower())
                || data.Category.ToLower().Contains(_imageFlowFilterString.ToLower())
                || data.Type.ToLower().Contains(_imageFlowFilterString.ToLower());
        }
        private string _imageFlowFilterString = "";
        public string ImageFlowFilterString
        {
            get { return _imageFlowFilterString; }
            set
            {
                _imageFlowFilterString = value;
                OnPropertyChanged("ImageFlowFilterString");
                ImageFlowItemsView.Refresh();
            }
        }

        #endregion FilteringAndSorting

        #region DataBaseManagement

        public void UpdateDataBase(DataMessage message)
        {
            var r = DataBase.FirstOrDefault(x => x.DataType == message.DataType && x.Key == message.Key);
            if (r != null)
                DataBase.Remove(r);
            DataBase.Add(message);
        }
        public String GetValueFromDataBase(String itemType, String itemKey, String path)
        {
            var r = DataBase.FirstOrDefault(x => x.DataType == itemType && x.Key == itemKey);
            if (r != null)
                return r.GetValue(path);
            return "";
        }

        #endregion DataBaseManagement

        #region ImagesAndPreviews

        private ManagerImageReference _mainimage;

        public ManagerImageReference MainImage
        {
            get
            {
                return _mainimage;
            }
            set
            {
                _mainimage = value;
                OnPropertyChanged("MainImage");
                OnPropertyChanged("MainImageSource");
                DispatchMainImage();
            }
        }

        public ImageSource MainImageSource
        {
            get
            {
                if (MainImage != null) return MainImage.PreviewImageSource;
                return null;
            }
        }

        private ManagerImageReference _previewimage;
        public ManagerImageReference PreviewImage { get { return _previewimage; } set { _previewimage = value; OnPropertyChanged("PreviewImage"); OnPropertyChanged("PreviewImageSource"); } }

        public ImageSource PreviewImageSource
        {
            get
            {
                if (PreviewImage != null) return PreviewImage.PreviewImageSource;
                return null;
            }
        }


        private ManagerImageReference _editorimage;
        public ManagerImageReference EditorImage { get { return _editorimage; } set { _editorimage = value; OnPropertyChanged("EditorImage"); OnPropertyChanged("EditorImageSource"); } }

        public ImageSource EditorImageSource
        {
            get
            {
                if (EditorImage != null) return EditorImage.PreviewImageSource;
                return null;
            }
        }

        #endregion ImagesAndPreviews

        private void DispatchMainImage()
        {
            // Main Image changed, i.e. Push to MQ !

            // Dispatch to Dispatcher for publication
            // DispatchNotificationMessage
            var m = new DispatchNotificationMessage()
            {
                Account = "EBU.io",
                ContentType = "application/json",
                Imageurl = MainImage.PublicImageUrl,
                Link = MainImage.Link,
                NotificationKey = Guid.NewGuid().ToString(),
                NotificationMessage = "Dispatch Message",
                ReceiveTime = DateTime.Now,
                Source = "EBU.io EIS Content Manager",
                Title = ""
            };

            _dataOutConnection.Dispatch(m);
        }


        public void OnReceive(DataMessage message)
        {
            if (_config.DataConfiguration.DataFlowTypes.Split(';').Contains(message.DataType))
            {
                // TODO Generalize Message creation
                // Add the message to the data flow
                var d = new DataFlowItem()
                {
                    DataMessage = message,
                    Name = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).NamePath),
                    Category = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).CategoryPath),
                    Type = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).TypePath),
                    Short = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).ShortPath),
                    Priority = DataFlowPriority.Low,
                    Timestamp = DateTime.Now
                };

                DataFlowItems.Add(d);
            }
            if (_config.DataConfiguration.ImageFlowTypes.Split(';').Contains(message.DataType))
            {
                // Add the message to the image flow
                var d = new DataFlowItem()
                {
                    DataMessage = message,
                    Name = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).NamePath),
                    Category = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).CategoryPath),
                    Type = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).TypePath),
                    Short = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).ShortPath),
                    Url = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).UrlPath),
                    Priority = DataFlowPriority.Low,
                    Timestamp = DateTime.Now
                };
                if (string.IsNullOrEmpty(d.Name))
                {
                    //Generate random name
                    d.Name = Guid.NewGuid().ToString() + ".jpg";
                }
                // Save the image to local folder
                if (d.Url.StartsWith("http"))
                {
                    var filepath = Path.Combine(_config.DataConfiguration.IncomingPictureFolder, d.Name);
                    var dli = new Thread(() => DownloadImageToLocal(d.Url, filepath));
                    dli.Start();
                }

                ImageFlowItems.Add(d);
            }

            if (_config.DataConfiguration.DataBaseTypes.Split(';').Contains(message.DataType))
            {
                // Add the message to the database
                UpdateDataBase(message);
            }
        }

        private void DownloadImageToLocal(string url, string localpath)
        {
            try
            {
                using (WebClient myWebClient = new WebClient())
                {
                    // Download the Web resource and save it into the current filesystem folder.
                    myWebClient.DownloadFile(url, localpath);
                }
            }
            catch (Exception) { }
        }

        #region DummyData
        public void DummyData()
        {
            // Dummy Data
            //EventFlow e0 = new EventFlow() { Name = "General" };
            //EventFlow e1 = new EventFlow() { Name = "100M Men" };
            //EventFlow e2 = new EventFlow() { Name = "High Jump Women" };
            //EventFlow e3 = new EventFlow() { Name = "4x100m Relay Women" };
            //EventFlow e4 = new EventFlow() { Name = "400m Hurdles Men" };

            //RunningEvents.Add(e0);
            //RunningEvents.Add(e1);
            //RunningEvents.Add(e2);
            //RunningEvents.Add(e3);
            //RunningEvents.Add(e4);

            DataFlowItem d1 = new DataFlowItem() { Name = "Data 15", Category = "100M", Type = "Results", Short = "Short text describing this", Priority = DataFlowPriority.High };
            DataFlowItem d2 = new DataFlowItem() { Name = "Data 25", Category = "ABC", Type = "Results", Short = "Short text describing this", Priority = DataFlowPriority.Medium };
            DataFlowItem d3 = new DataFlowItem() { Name = "Data 35", Category = "DDC", Type = "StartList", Short = "Short text describing this", Priority = DataFlowPriority.Low };
            DataFlowItem d4 = new DataFlowItem() { Name = "Data 45", Category = "DDC", Type = "Results", Short = "Short text describing this", Priority = DataFlowPriority.Neglectable };
            DataFlowItem d5 = new DataFlowItem() { Name = "Data 55", Category = "DDC", Type = "Official Results", Short = "Short text describing this", Priority = DataFlowPriority.Medium };
            DataFlowItem d6 = new DataFlowItem() { Name = "Data 65", Category = "DDC", Type = "Results", Short = "Short text describing this", Priority = DataFlowPriority.Neglectable };
            DataFlowItem d7 = new DataFlowItem() { Name = "Data 65", Category = "WEATHER", Type = "WEATHER", Short = "Stadium", Priority = DataFlowPriority.Neglectable };

            DataFlowItems.Add(d1);
            DataFlowItems.Add(d2);
            DataFlowItems.Add(d3);
            DataFlowItems.Add(d4);
            DataFlowItems.Add(d5);
            DataFlowItems.Add(d6);
            DataFlowItems.Add(d7);


            DataFlowItem i1 = new DataFlowItem()
            {
                Name = "Image 1",
                Category = "Getty",
                Type = "Image",
                Short = "Short text describing this",
                Priority = DataFlowPriority.High,
                Url = @"Z:\mh On My Mac\EBU\Assets\GETTY Images Day 1 Braunschweig\450987746.jpg"
            };
            DataFlowItem i2 = new DataFlowItem()
            {
                Name = "Image 2",
                Category = "Getty",
                Type = "Image",
                Short = "Short text describing this",
                Priority = DataFlowPriority.Medium,
                Url = @"Z:\mh On My Mac\EBU\Assets\GETTY Images Day 1 Braunschweig\450987748.jpg"
            };
            DataFlowItem i3 = new DataFlowItem()
            {
                Name = "Image 3",
                Category = "Getty",
                Type = "Image",
                Short = "Short text describing this",
                Priority = DataFlowPriority.Low,
                Url = @"Z:\mh On My Mac\EBU\Assets\GETTY Images Day 1 Braunschweig\450987750.jpg"
            };

            ImageFlowItems.Add(i1);
            ImageFlowItems.Add(i2);
            ImageFlowItems.Add(i3);

        }
        #endregion DummyData

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
