using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using io.ebu.eis.canvasgenerator;
using io.ebu.eis.data.file;
using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;
using io.ebu.eis.http;
using io.ebu.eis.mq;
using io.ebu.eis.shared;

namespace io.ebu.eis.contentmanager
{
    [DataContract]
    public class ManagerContext : INotifyPropertyChanged, IDataMessageHandler, IDisposable, ISystemFileRouter, IImageGenerationHandler
    {
        private readonly CMConfigurationSection _config;
        public CMConfigurationSection Config { get { return _config; } }

        private readonly List<AMQQueuePublisher> _publishers = new List<AMQQueuePublisher>();
        private readonly AMQQueuePublisher _imageGeneratorMq;
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private bool _inAutomationMode;
        [DataMember(Name = "inautomationmode")]
        public bool InAutomationMode { get { return _inAutomationMode; } set { _inAutomationMode = value; OnPropertyChanged("InAutomationMode"); } }

        private int _automationInterval = 15;
        [DataMember(Name = "automationinterval")]
        public int AutomationInterval { get { return _automationInterval; } set { _automationInterval = value; OnPropertyChanged("AutomationInterval"); } }

        private double _automationProgress;
        public double AutomationProgress { get { return _automationProgress; } set { _automationProgress = value; OnPropertyChanged("AutomationProgress"); } }

        private bool _isInOverrideCart;
        public bool IsInOverrideCart { get { return _isInOverrideCart; } set { _isInOverrideCart = value; OnPropertyChanged("IsInOverrideCart"); } }

        private bool _allowOverride = true;
        [DataMember(Name = "allowoverride")]
        public bool AllowOverride { get { return _allowOverride; } set { _allowOverride = value; OnPropertyChanged("AllowOverride"); } }

        private int _overrideSlideCountDown;
        public int OverrideSlideCountDown { get { return _overrideSlideCountDown; } set { _overrideSlideCountDown = value; OnPropertyChanged("OverrideSlideCountDown"); } }

        private int _overrideRotationCount = 3;
        [DataMember(Name = "overriderotationcount")]
        public int OverrideRotationCount { get { return _overrideRotationCount; } set { _overrideRotationCount = value; OnPropertyChanged("OverrideRotationCount"); } }

        private double _overrideProgress;
        public double OverrideProgress { get { return _overrideProgress; } set { _overrideProgress = value; OnPropertyChanged("OverrideProgress"); } }

        private bool _editModeEnabled;
        [DataMember(Name = "editmodeenabled", IsRequired = false)]
        public bool EditModeEnabled
        {
            get { return _editModeEnabled; }
            set
            {
                _editModeEnabled = value;
                OnPropertyChanged("EditModeEnabled");
            }
        }


        public bool IsPreviewCartOffAir { get { return ActiveCart != PreviewCart; } }
        public bool IsPreviewCartOnAir { get { return ActiveCart == PreviewCart; } }

        private ManagerCart _activeCart;
        public ManagerCart ActiveCart
        {
            get { return _activeCart; }
            set
            {
                _activeCart = value;
                ResetAllCartsInactive();
                OnPropertyChanged("ActiveCart"); OnPropertyChanged("IsPreviewCartOffAir"); OnPropertyChanged("IsPreviewCartOnAir");
            }
        }

        private ManagerCart _previewCart;
        public ManagerCart PreviewCart { get { return _previewCart; } set { _previewCart = value; OnPropertyChanged("PreviewCart"); OnPropertyChanged("IsPreviewCartOffAir"); OnPropertyChanged("IsPreviewCartOnAir"); } }

        private ManagerCart _editorCart;

        public ManagerCart EditorCart
        {
            get
            {
                if (_editorCart == null)
                {
                    _editorCart = new ManagerCart("Editor Cart");
                    OnPropertyChanged("EditorCart");
                }
                return _editorCart;
            }
            set { _editorCart = value; OnPropertyChanged("EditorCart"); }
        }


        private DispatchedObservableCollection<ManagerCart> _carts;
        [DataMember(Name = "carts")]
        public DispatchedObservableCollection<ManagerCart> Carts { get { return _carts; } set { _carts = value; OnPropertyChanged("Carts"); } }
        public ICollectionView CartItemsView;

        #region Database
        private DispatchedObservableCollection<DataMessage> _dataBase;
        [DataMember(Name = "database")]
        public DispatchedObservableCollection<DataMessage> DataBase { get { return _dataBase; } set { _dataBase = value; OnPropertyChanged("DataBase"); } }

        private DataMessage _globalData;
        [DataMember(Name = "GlobalData", IsRequired = false)]
        public DataMessage GlobalData { get { return _globalData; } set { _globalData = value; OnPropertyChanged("GlobalData"); } }

        //private DispatchedObservableCollection<EventFlow> _runningEvents;
        //public DispatchedObservableCollection<EventFlow> RunningEvents { get { return _runningEvents; } set { _runningEvents = value; OnPropertyChanged("RunningEvents"); } }

        private DispatchedObservableCollection<DataFlowItem> _dataFlowItems;
        [DataMember(Name = "dataflowitems")]
        public DispatchedObservableCollection<DataFlowItem> DataFlowItems { get { return _dataFlowItems; } set { _dataFlowItems = value; OnPropertyChanged("DataFlowItems"); } }
        public ICollectionView DataFlowItemsView;

        private DispatchedObservableCollection<DataFlowItem> _imageFlowItems;
        [DataMember(Name = "imageflowitems")]
        public DispatchedObservableCollection<DataFlowItem> ImageFlowItems { get { return _imageFlowItems; } set { _imageFlowItems = value; OnPropertyChanged("ImageFlowItems"); } }
        public ICollectionView ImageFlowItemsView;
        #endregion Database

        public ManagerContext()
        {
            _config = (CMConfigurationSection)ConfigurationManager.GetSection("CMConfiguration");

            GlobalData = new DataMessage();

            Carts = new DispatchedObservableCollection<ManagerCart>();
            CartItemsView = CollectionViewSource.GetDefaultView(Carts);
            CartItemsView.Filter = CartDislpayFilter;


            DataBase = new DispatchedObservableCollection<DataMessage>();
            //RunningEvents = new DispatchedObservableCollection<EventFlow>();
            DataFlowItems = new DispatchedObservableCollection<DataFlowItem>();
            ImageFlowItems = new DispatchedObservableCollection<DataFlowItem>();

            DataFlowItemsView = CollectionViewSource.GetDefaultView(DataFlowItems);
            DataFlowItemsView.Filter = DataFlowNameFilter;
            var sortData = new SortDescription { Direction = ListSortDirection.Descending, PropertyName = "Timestamp" };
            DataFlowItemsView.SortDescriptions.Add(sortData);

            ImageFlowItemsView = CollectionViewSource.GetDefaultView(ImageFlowItems);
            ImageFlowItemsView.Filter = ImageFlowNameFilter;
            var sortImage = new SortDescription { Direction = ListSortDirection.Descending, PropertyName = "Timestamp" };
            ImageFlowItemsView.SortDescriptions.Add(sortImage);

            #region INPUTS
            // Open Connection to INBOUND MQ
            foreach (InputConfiguration input in _config.InputConfigurations)
            {
                switch (input.Type.ToUpper())
                {
                    case "MQ":
                        {
                            var amquri = input.MQUri;
                            var amqinexchange = input.MQExchange;
                            var dataInConnection = new AMQTopicConsumer(amquri, amqinexchange, this);
                            dataInConnection.ConnectAsync();
                            // TODO Catch hand handle connection exceptions and reconnect

                            _disposables.Add(dataInConnection);
                        }
                        break;
                    case "HTTP":
                        {
                            var httpBindIp = input.BindIp;
                            var httpBindPort = input.BindPort;
                            var dataInHttpServer = new CMHttpServer(httpBindIp, httpBindPort, this);
                            dataInHttpServer.Start();

                            _disposables.Add(dataInHttpServer);
                        }
                        break;
                        // TODO Handle and log default
                }
            }

            // Callback from Image Generation
            if (_config.ImageGemerationConfiguration.EnableExternalImageGenerator)
            {
                // Input
                foreach (InputConfiguration input in _config.ImageGemerationConfiguration.InputConfigurations)
                {
                    switch (input.Type.ToUpper())
                    {
                        case "MQ":
                            {
                                var amquri = input.MQUri;
                                var amqqueue = input.MQQueue;
                                var dataInConnection = new AMQQueueConsumer(amquri, amqqueue, this);
                                dataInConnection.ConnectAsync();
                                // TODO Catch hand handle connection exceptions and reconnect

                                _disposables.Add(dataInConnection);
                            }
                            break;
                    }
                }
                // Output
                var amqImageGenerationUri = _config.ImageGemerationConfiguration.DispatchMQConfiguration.MQUri;
                var amqImageGenerationQueue = _config.ImageGemerationConfiguration.DispatchMQConfiguration.MQQueue;
                _imageGeneratorMq = new AMQQueuePublisher(amqImageGenerationUri, amqImageGenerationQueue);
                _imageGeneratorMq.Connect();

                _disposables.Add(_imageGeneratorMq);
            }
            #endregion INPUTS

            #region OUTPUTS
            // Open Connection to OUTBOUND MQ if enabled and URI defined
            var amqDispatchUri = _config.OutputConfiguration.DispatchMQConfiguration.MQUri;
            if (_config.OutputConfiguration.EnableDataDispatchMQ && !string.IsNullOrEmpty(amqDispatchUri))
            {
                var amqoutexchange = _config.OutputConfiguration.DispatchMQConfiguration.MQExchange;
                var dataOutConnection = new AMQQueuePublisher(amqDispatchUri, amqoutexchange);
                dataOutConnection.ConnectAsync();
                _publishers.Add(dataOutConnection);
                // TODO Catch hand handle connection exceptions and reconnect

                _disposables.Add(dataOutConnection);
            }
            #endregion OUTPUTS

            if (!ResetFomrFile())
            {
                LoadCarts(true, false);
                LoadInitialImages();
                LoadReceivedImages();
            }
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }

        public void SerializeToFile()
        {
            if (!string.IsNullOrEmpty(_config.DataConfiguration.StateConfigurationFile))
            {
                var json = JsonSerializer.Serialize(this);
                File.WriteAllText(_config.DataConfiguration.StateConfigurationFile, json);
            }
        }

        public bool ResetFomrFile()
        {
            if (!string.IsNullOrEmpty(_config.DataConfiguration.StateConfigurationFile) &&
                File.Exists(_config.DataConfiguration.StateConfigurationFile))
            {
                var json = File.ReadAllText(_config.DataConfiguration.StateConfigurationFile);
                try
                {
                    var oldconf = JsonSerializer.Deserialize<ManagerContext>(json);

                    foreach (var c in oldconf.Carts)
                    {
                        Carts.Add(c);
                    }

                    LoadCarts(false, false);

                    foreach (var d in oldconf.DataBase)
                    {
                        DataBase.Add(d);
                    }
                    foreach (var d in oldconf.DataFlowItems)
                    {
                        DataFlowItems.Add(d);
                    }
                    foreach (var i in oldconf.ImageFlowItems)
                    {
                        if (i.Url.StartsWith("http") || File.Exists(i.Url))
                        {
                            // Only reload web and local images that exist
                            ImageFlowItems.Add(i);
                        }
                    }

                    InAutomationMode = oldconf.InAutomationMode;
                    AutomationInterval = oldconf.AutomationInterval;
                    AllowOverride = oldconf.AllowOverride;
                    OverrideRotationCount = oldconf.OverrideRotationCount;

                    ActiveCart = Carts.FirstOrDefault(x => x.Name == "INITIAL");
                    PreviewCart = Carts.FirstOrDefault(x => x.Name == "INITIAL");
                    EditorCart = Carts.FirstOrDefault(x => x.Name == "ALL");

                    ResetRendererAndConfig();

                    return true;
                }
                catch (Exception)
                {
                    // TODO Log
                }
            }
            return false;
        }

        private void ResetRendererAndConfig()
        {
            foreach (var c in Carts)
            {
                foreach (var s in c.Slides)
                {
                    s.Config = _config;
                    s.ImageGenerationHandler = this;
                }
            }
        }

        public void LoadCarts(bool loadAll, bool cleanReload)
        {
            if (cleanReload)
            {
                // Cleanup existing
                Carts.Clear();
            }

            foreach (CartConfiguration cart in _config.SlidesConfiguration.CartConfigurations)
            {
                if (loadAll || Carts.Count(x => x.Name == cart.Name) == 0)
                {
                    var c = new ManagerCart(cart.Name)
                    {
                        CanBeDeleted = false,
                        ShowInCartList = cart.ShowInCartList
                    };

                    Carts.Add(c);

                    foreach (SlideConfiguration s in cart.Slides)
                    {
                        var sl = new ManagerImageReference(_config, this)
                        {
                            Template = s.Filename,
                            Link = s.DefaultLink,
                            Text = s.DefaultText,
                            RegenerateOnPublish = s.RegenerateOnPublish,
                            ValidityPeriod = TimeSpan.FromSeconds(s.ValidityPeriodSeconds),
                            CanRepeate = s.CanRepeat,
                            ItemsPerSlide = s.ItemsPerSlide,
                            Context = new DataMessage()
                        };
                        c.Slides.Add(sl);
                    }

                    if (cart.Active)
                    {
                        // Set current cart as active
                        ActiveCart = c;
                        PreviewCart = c;
                    }
                    if (cart.EditorDefault)
                    {
                        // Set current editor cart
                        EditorCart = c;
                    }
                }
            }
        }

        public void AddEditorTemplate(string path)
        {
            var newTemplate = new ManagerImageReference(_config, this)
            {
                Template = path,
                Link = _config.SlidesConfiguration.DefaultLink,
                CanRepeate = false
            };

            EditorCart.Slides.Add(newTemplate);
        }

        private void ResetAllCartsInactive()
        {
            foreach (var c in Carts)
            {
                c.IsActive = false;
            }
            if (ActiveCart != null)
            {
                ActiveCart.IsActive = true;
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

        protected virtual bool IsFileLocked(string filePath)
        {
            var file = new FileInfo(filePath);
            // From http://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public void RouteFile(string filepath)
        {
            // Route the file as image to Ingest

            // Add a delay
            while (IsFileLocked(filepath))
            {
                Thread.Sleep(500);
            }
            IngestImage(filepath);
        }
        public void IngestImage(string file)
        {
            try
            {
                var title = new DataMessage() { Key = "TITLE", Value = Path.GetFileName(file) };
                var url = new DataMessage() { Key = "URL", Value = file };
                var type = new DataMessage() { Key = "TYPE", Value = Path.GetExtension(file) };
                var im = new DataMessage()
                {
                    DataType = "IMAGE",
                    Key = Path.GetFileName(file),
                    Value = file,
                };
                im.Data.Add(title);
                im.Data.Add(url);
                im.Data.Add(type);

                OnReceive(im);
            }
            catch (Exception)
            {
                // TODO Log
            }
        }

        public void SwitchToNextSlide()
        {
            if (ActiveCart != null && ActiveCart.Slides.Count > 0)
            {
                // We can switch to the next one.
                MainImage = ActiveCart.GetNextSlide();
                ReloadPreview();

                if (OverrideSlideCountDown > 0)
                    --OverrideSlideCountDown;
            }
        }

        public void ReloadPreview()
        {
            if (ActiveCart != null)
            {
                PreviewImage = ActiveCart.PreviewNextSlide();

                // Pregenerate Preview
                if (PreviewImage.RegenerateOnPublish && !PreviewImage.IsValid)
                    PreviewImage.ReRender(true);
            }
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

        private bool CartDislpayFilter(object item)
        {
            ManagerCart data = item as ManagerCart;
            return data != null && data.ShowInCartList;
        }

        /** DATAFLOW Filter **/
        private bool DataFlowNameFilter(object item)
        {
            DataFlowItem data = item as DataFlowItem;
            return data != null && (data.Name.ToLower().Contains(_dataFlowFilterString.ToLower())
                                    || data.Category.ToLower().Contains(_dataFlowFilterString.ToLower())
                                    || data.Type.ToLower().Contains(_dataFlowFilterString.ToLower()));
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
            return data != null && (data.Name.ToLower().Contains(_imageFlowFilterString.ToLower())
                                    || data.Category.ToLower().Contains(_imageFlowFilterString.ToLower())
                                    || data.Type.ToLower().Contains(_imageFlowFilterString.ToLower()));
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
                DispatchMainImage();
            }
        }

        private ManagerImageReference _previewimage;

        public ManagerImageReference PreviewImage
        {
            get
            {
                return _previewimage;
            }
            set
            {
                _previewimage = value;
                OnPropertyChanged("PreviewImage");
            }
        }


        private ManagerImageReference _editorimage;

        public ManagerImageReference EditorImage
        {
            get
            {
                return _editorimage;
            }
            set
            {
                _editorimage = value;
                OnPropertyChanged("EditorImage");
            }
        }


        #endregion ImagesAndPreviews

        private void DispatchMainImage()
        {
            // Main Image changed, i.e. Push to MQ !

            // Regenerate Forced Image Rerendering on Publish Images
            if (MainImage.RegenerateOnPublish && !MainImage.IsValid)
            {
                MainImage.ReRender(true);
            }

            // Direct Dispatch
            MainImage.DispatchLastUpload();

            // Dispatch to Dispatcher for publication
            // DispatchNotificationMessage
            var m = new DispatchNotificationMessage()
            {
                Account = "EBU",
                ContentType = "application/json",
                Imageurl = MainImage.PublicImageUrl,
                Link = MainImage.Link,
                NotificationKey = Guid.NewGuid().ToString(),
                NotificationMessage = "Dispatch Message",
                ReceiveTime = DateTime.Now,
                Source = "EBU EIS Content Manager",
                Title = MainImage.Text,
                ImageVariants = MainImage.ImageVariants
            };

            foreach (var pub in _publishers)
            {
                pub.Dispatch(m);
            }
        }

        public void ResetAllPriorities()
        {
            foreach (var d in DataFlowItems)
            {
                SetFlowItemPriority(d);
                d.Name = d.DataMessage.GetValue(_config.DataConfiguration.GetPathByDataType(d.DataMessage.DataType).NamePath);
                d.Category = d.DataMessage.GetValue(_config.DataConfiguration.GetPathByDataType(d.DataMessage.DataType).CategoryPath);
                d.Type = d.DataMessage.GetValue(_config.DataConfiguration.GetPathByDataType(d.DataMessage.DataType).TypePath);
                d.Short = d.DataMessage.GetValue(_config.DataConfiguration.GetPathByDataType(d.DataMessage.DataType).ShortPath);
            }
            foreach (var d in ImageFlowItems)
            {
                SetFlowItemPriority(d);
                d.Name = d.DataMessage.GetValue(_config.DataConfiguration.GetPathByDataType(d.DataMessage.DataType).NamePath);
                d.Category = d.DataMessage.GetValue(_config.DataConfiguration.GetPathByDataType(d.DataMessage.DataType).CategoryPath);
                d.Type = d.DataMessage.GetValue(_config.DataConfiguration.GetPathByDataType(d.DataMessage.DataType).TypePath);
                d.Short = d.DataMessage.GetValue(_config.DataConfiguration.GetPathByDataType(d.DataMessage.DataType).ShortPath);
            }
        }

        private void SetFlowItemPriority(DataFlowItem item)
        {
            foreach (DataPriorityConfiguration prioconf in _config.DataConfiguration.DataPriorityConfigurations)
            {
                var value = item.DataMessage.GetValue(prioconf.DataPath);
                switch (prioconf.Operator.ToLower())
                {
                    case "equals":
                        if (String.Compare(value, prioconf.ExpectedValue, StringComparison.Ordinal) == 0)
                        {
                            item.Priority = StringToDataFlowPriority(prioconf.Priority);
                        }
                        break;
                    case "startswith":
                        if (value.StartsWith(prioconf.ExpectedValue))
                        {
                            item.Priority = StringToDataFlowPriority(prioconf.Priority);
                        }
                        break;
                    case "endswith":
                        if (value.EndsWith(prioconf.ExpectedValue))
                        {
                            item.Priority = StringToDataFlowPriority(prioconf.Priority);
                        }
                        break;
                    case "contains":
                        if (value.Contains(prioconf.ExpectedValue))
                        {
                            item.Priority = StringToDataFlowPriority(prioconf.Priority);
                        }
                        break;
                }
            }
        }

        private bool DataDoesAutoOnAirCart(DataFlowItem item)
        {

            foreach (OnAirCartAutoCondition cond in _config.DataConfiguration.OnAirCartAutoConfiguration)
            {
                var value = item.DataMessage.GetValue(cond.DataPath);
                switch (cond.Operator.ToLower())
                {
                    case "equals":
                        if (String.Compare(value, cond.ExpectedValue, StringComparison.Ordinal) == 0) return true;
                        break;
                    case "startswith":
                        if (value.StartsWith(cond.ExpectedValue)) return true;
                        break;
                    case "endswith":
                        if (value.EndsWith(cond.ExpectedValue)) return true;
                        break;
                    case "contains":
                        if (value.Contains(cond.ExpectedValue)) return true;
                        break;
                }
            }

            // If we ended up here, item corresponds to all conditions
            return false;
        }

        private DataFlowPriority StringToDataFlowPriority(string input)
        {
            switch (input.ToLower())
            {
                case "high": return DataFlowPriority.High;
                case "medium": return DataFlowPriority.Medium;
                case "low": return DataFlowPriority.Low;
                case "green": return DataFlowPriority.Green;
                case "neglectable": return DataFlowPriority.Neglectable;
            }
            return DataFlowPriority.Unknown;
        }

        public void CreateCartForDataFlowItem(DataFlowItem m, ManagerCart appentToThisCart)
        {

            var conf = Config.DataConfiguration.DataItemConfigurations.Cast<DataItemConfiguration>().FirstOrDefault(c => c.DataType == m.DataMessage.DataType);

            // Configure which DataTypes generate Carts
            //var cartName = "";
            //var types = _config.DataConfiguration.AutoCartGenerationTypes.Split(';');
            //if (types.Contains(m.DataMessage.DataType))
            //    cartName = m.DataMessage.DataType;
            if (conf != null)
            {
                var existingCart = Carts.FirstOrDefault(x => x.Name.ToUpper() == conf.DefaultCartName.ToUpper());
                if (existingCart != null)
                {
                    // TODO ASK IF Switch Cart or add to cart

                    var newCart = existingCart.Clone();
                    newCart.Name = newCart.Name + " - " + m.DataMessage.GetValue("EVENTNAME");
                    if (newCart.Slides.Count == 1 && newCart.Slides.First().CanRepeate)
                    {
                        if (conf.DefaultCartName.ToUpper() == "STARTLIST")
                        {
                            // We might need to repeate the slide due to data and slit the context
                            // We assume 4 line per slide
                            // TODO This needs to be generic !
                            var context = m.DataMessage.Clone();

                            var ath = context.Data.FirstOrDefault(x => x.Key == "STARTPOSITIONS");

                            // Sort the list first try by int then by alpha num
                            bool ordered = false;
                            try
                            {
                                if (ath != null)
                                {
                                    ath.Data =
                                        ath.Data.OrderBy(s => string.IsNullOrWhiteSpace(s.Key))
                                            .ThenBy(x => Convert.ToInt32("0" + x.Key))
                                            .ToList();
                                    ordered = true;
                                }
                            }
                            catch (Exception)
                            {
                                // TODO Log
                            }
                            if (!ordered)
                            {
                                // Try alpha order
                                try
                                {
                                    if (ath != null)
                                    {
                                        ath.Data =
                                            ath.Data.OrderBy(s => string.IsNullOrWhiteSpace(s.Key))
                                                .ThenBy(x => x.Key)
                                                .ToList();
                                    }
                                }
                                catch (Exception)
                                {
                                    // TODO Log
                                }
                            }


                            newCart.Slides.First().Context = context;
                            newCart.Slides.First().ReRender(GlobalData);

                            var itemsPerSlide = newCart.Slides.First().ItemsPerSlide;
                            var offset = itemsPerSlide;
                            while (ath != null && ath.Data.Count > itemsPerSlide)
                            {
                                var clone = ath.Clone();
                                clone.Data.RemoveRange(0, itemsPerSlide);
                                ath.Data.RemoveRange(itemsPerSlide, ath.Data.Count - itemsPerSlide);
                                var newSlide = newCart.Slides.First().Clone(false);
                                newSlide.IndexOffset = offset;
                                var contextClone = context.Clone();
                                contextClone.Data.First(x => x.Key == "STARTPOSITIONS").Data = clone.Data;
                                newSlide.Context = contextClone;
                                newSlide.ReRender(GlobalData);
                                newCart.Slides.Add(newSlide);
                                ath = clone;
                                offset = offset + itemsPerSlide;
                            }
                        }
                        else if (conf.DefaultCartName.ToUpper() == "RESULTLIST")
                        {
                            // We might need to repeate the slide due to data and slit the context
                            // We assume 4 line per slide
                            // TODO This needs to be generic !
                            var context = m.DataMessage.Clone();

                            var ath = context.Data.FirstOrDefault(x => x.Key == "RESULTS");

                            // Sort the list first try by int then by alpha num
                            bool ordered = false;
                            try
                            {
                                if (ath != null)
                                {
                                    ath.Data =
                                        ath.Data.OrderBy(s => string.IsNullOrWhiteSpace(s.Key))
                                            .ThenBy(x => Convert.ToInt32("0" + x.Key))
                                            .ToList();

                                    ordered = true;
                                }
                            }
                            catch (Exception)
                            {
                                // TODO Log
                            }
                            if (!ordered)
                            {
                                // Try alpha order
                                try
                                {
                                    if (ath != null)
                                    {
                                        ath.Data =
                                            ath.Data.OrderBy(s => string.IsNullOrWhiteSpace(s.Key))
                                                .ThenBy(x => x.Key)
                                                .ToList();
                                    }
                                }
                                catch (Exception)
                                {
                                    // TODO Log
                                }
                            }

                            newCart.Slides.First().Context = context;
                            newCart.Slides.First().ReRender(GlobalData);
                            var itemsPerSlide = newCart.Slides.First().ItemsPerSlide;
                            var offset = itemsPerSlide;
                            while (ath != null && ath.Data.Count > itemsPerSlide)
                            {
                                var clone = ath.Clone();
                                clone.Data.RemoveRange(0, itemsPerSlide);
                                ath.Data.RemoveRange(itemsPerSlide, ath.Data.Count - itemsPerSlide);
                                var newSlide = newCart.Slides.First().Clone(false);
                                newSlide.IndexOffset = offset;
                                var contextCLone = context.Clone();
                                contextCLone.Data.First(x => x.Key == "RESULTS").Data = clone.Data;
                                newSlide.Context = contextCLone;
                                newSlide.ReRender(GlobalData);
                                newCart.Slides.Add(newSlide);
                                ath = clone;
                                offset = offset + itemsPerSlide;
                            }
                        }
                        else
                        {
                            // Set Context for all slides
                            var context = m.DataMessage.Clone();
                            foreach (var s in newCart.Slides)
                            {
                                s.Context = context;
                                s.ReRender(GlobalData);
                            }
                        }
                    }
                    else
                    {
                        foreach (var s in newCart.Slides)
                        {
                            // Set the context on all slides
                            s.Context = m.DataMessage;
                            s.ReRender(GlobalData);
                        }
                    }

                    if (appentToThisCart != null)
                    {
                        // Append i.e. merge to current
                        foreach (var s in newCart.Slides)
                        {
                            // Add to open cart (not the currently active one)
                            appentToThisCart.Slides.Add(s);
                        }
                    }
                    else
                    {
                        // Generate new cart otherwise
                        Carts.Add(newCart);
                        PreviewCart = newCart;
                    }
                }
                else
                {
                    // No Cart, try with template only
                    if (!string.IsNullOrEmpty(conf.DefaultTemplate))
                    {
                        // Create new ImageRef
                        var newImgRef = new ManagerImageReference(Config, this)
                        {
                            Template = conf.DefaultTemplate,
                            Link = Config.SlidesConfiguration.DefaultLink,
                            Context = m.DataMessage,
                            CanRepeate = false
                        };
                        // Handle
                        if (appentToThisCart != null)
                        {
                            // Add to open cart (not the currently active one)
                            appentToThisCart.Slides.Add(newImgRef);
                        }
                        else
                        {
                            var newCart = new ManagerCart("TEMP " + conf.DataType);
                            newCart.Slides.Add(newImgRef);
                            Carts.Add(newCart);
                            PreviewCart = newCart;
                        }
                    }
                }
            }
            else
            {
                // No cart defined for that event type
            }
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
                    Priority = DataFlowPriority.Neglectable,
                    Timestamp = DateTime.Now
                };
                if (!string.IsNullOrEmpty(_config.DataConfiguration.GetPathByDataType(message.DataType).UrlPath))
                {
                    // Set Url if available
                    d.Url = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).UrlPath);
                }
                SetFlowItemPriority(d);
                DataFlowItems.Add(d);

                // Cleanup
                while (DataFlowItems.Count > 300)
                {
                    DataFlowItems.RemoveAt(0);
                }

                // Handle auto
                if (DataDoesAutoOnAirCart(d) && AllowOverride)
                {
                    // Create the cart
                    // CreateCartForDataFlowItem(d, false);
                    // Put cart on air
                    // ActiveCart = PreviewCart;
                    // ReloadPreview();

                    // IsInOverrideCart = true;
                    // OverrideProgress = 1.0;
                    // OverrideSlideCountDown = ActiveCart.Slides.Count * OverrideRotationCount;

                    var autoCart = Carts.FirstOrDefault(x => x.Name == "AUTO");
                    if (autoCart == null)
                    {
                        autoCart = new ManagerCart("AUTO");
                        Carts.Insert(0, autoCart);
                    }

                    // TODO NEW HANDLING
                    if (Config.DataConfiguration.AutoCartClearTypes.Split(';').Contains(d.DataMessage.DataType))
                    {
                        // Need to clear the current active cart
                        autoCart.Slides.Clear();
                    }
                    if (Config.DataConfiguration.AutoCartAppendTypes.Split(';').Contains(d.DataMessage.DataType))
                    {
                        // Need to append to current active cart
                        CreateCartForDataFlowItem(d, autoCart);
                    }
                    if (Config.DataConfiguration.AutoCartGenerationTypes.Split(';').Contains(d.DataMessage.DataType))
                    {
                        // Need to create a new cart
                        CreateCartForDataFlowItem(d, autoCart);
                    }
                    if (Config.DataConfiguration.AutoEditorTypes.Split(';').Contains(d.DataMessage.DataType))
                    {
                        // Need to update the editor's template
                        if (Config.DataConfiguration.DataFlowLeftActions.Split(';').Contains("Template"))
                        {
                            // Load template to editor
                            // Find appropriate template
                            var conf = Config.DataConfiguration.DataItemConfigurations.Cast<DataItemConfiguration>().FirstOrDefault(c => c.DataType == d.DataMessage.DataType);
                            // Load corresponding template
                            if (conf != null)
                            {
                                var newTemplate = new ManagerImageReference(Config, this)
                                {
                                    Template = conf.DefaultTemplate,
                                    Link = Config.SlidesConfiguration.DefaultLink,
                                    CanRepeate = false
                                };

                                EditorImage = newTemplate;
                            }
                        }
                        if (Config.DataConfiguration.DataFlowLeftActions.Split(';').Contains("BackgroundUrl"))
                        {
                            // Set the background Uri
                            EditorImage.Background = d.Url;
                        }
                        if (Config.DataConfiguration.DataFlowLeftActions.Split(';').Contains("Context"))
                        {
                            // Set the context
                            EditorImage.Context = d.DataMessage;
                            EditorImage.ReRender(GlobalData);
                        }
                        EditorImage.ReRender(true);
                        EditorImage = EditorImage;
                    }
                }
            }
            if (_config.DataConfiguration.ImageFlowTypes.Split(';').Contains(message.DataType) ||
                (string.IsNullOrEmpty(_config.DataConfiguration.ImageFlowTypes) && message.DataType.CompareTo("IMAGE") == 0))
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
                    Priority = DataFlowPriority.Neglectable,
                    Timestamp = DateTime.Now
                };

                // Handle dupplicates in http
                if (ImageFlowItems.Any(x => x.Timestamp > DateTime.Now.AddMinutes(-1) && x.Url == d.Url))
                {
                    // We already have it since less than a minute thus return and ignore it
                    return;
                }
                // Handle empty names
                if (string.IsNullOrEmpty(d.Name))
                {
                    //Generate random name
                    d.Name = Guid.NewGuid().ToString() + ".jpg";
                }
                if (d.Name.StartsWith("INSTAGRAM"))
                {
                    d.Name = Guid.NewGuid().ToString() + "_instagram.jpg";
                }
                // Save the image to local folder
                if (d.Url.StartsWith("http"))
                {
                    var filepath = Path.Combine(_config.DataConfiguration.IncomingPictureFolder, d.Name);
                    var dli = new Thread(() => DownloadImageToLocal(d.Url, filepath));
                    dli.Start();
                }

                // Set the priority according to config
                SetFlowItemPriority(d);

                // Add element to the list
                ImageFlowItems.Add(d);


                // Cleanup, keep the last 200 images only
                while (ImageFlowItems.Count > 200)
                {
                    ImageFlowItems.RemoveAt(0);
                }
            }

            if (_config.DataConfiguration.DataBaseTypes.Split(';').Contains(message.DataType))
            {
                // Add the message to the database
                UpdateDataBase(message);
            }
        }

        #region RemoteFunctions

        public void UpdateGlobalData(DataMessage message)
        {
            var rerender = false;
            if (GlobalData == null)
            {
                GlobalData = message.Clone();
                rerender = true;
            }
            else
            {
                rerender = GlobalData.MergeGlobal(message);
            }

            if (rerender)
            {
                // Some data changed
                // Reset all Slides for new Data
                InvalidateSlidesWithGlobalData();
            }
        }

        public void ClearActiveCart()
        {
            if (ActiveCart.Slides.Any())
            {
                ActiveCart.Slides.Clear();
            }
        }

        public void AddSlides(List<string> slideNames)
        {
            // For all slide names
            foreach (var slideName in slideNames)
            {
                // Find Slides that match that name
                foreach (CartConfiguration cart in _config.SlidesConfiguration.CartConfigurations)
                {
                    foreach (SlideConfiguration s in cart.Slides)
                    {
                        if (s.Filename.StartsWith(slideName))
                        {
                            var sl = new ManagerImageReference(_config, this)
                            {
                                Template = s.Filename,
                                Link = s.DefaultLink,
                                Text = s.DefaultText,
                                RegenerateOnPublish = s.RegenerateOnPublish,
                                ValidityPeriod = TimeSpan.FromSeconds(s.ValidityPeriodSeconds),
                                CanRepeate = s.CanRepeat,
                                ItemsPerSlide = s.ItemsPerSlide,
                                Context = new DataMessage()
                            };
                            // Add the slide to the active cart
                            ActiveCart.Slides.Add(sl);
                        }
                    }
                }
            }
        }


        public void BroadcastSlide(string slideName)
        {
            // Find right slide
            var slide = ActiveCart.Slides.FirstOrDefault(x => x.Template.StartsWith(slideName));
            // Switch
            if (slide != null)
            {
                SwitchToSlide(slide);
            }

        }

        #endregion RemoteFunctions

        public void InvalidateSlidesWithGlobalData()
        {
            // Cart List
            foreach (var c in Carts)
            {
                if (c.ShowInCartList)
                {
                    foreach (var s in c.Slides)
                    {
                        if (c.IsActive)
                        {
                            // Rerender active
                            s.ReRender(GlobalData);
                        }
                        else
                        {
                            // Only update others
                            s.UpdateGlobal(GlobalData);
                        }
                    }
                }
            }

            // Don't recompute Editor or preview Carts since no use
            // Preview Cart
            foreach (var s in PreviewCart.Slides)
            {
                s.UpdateGlobal(GlobalData);
            }
            foreach (var s in EditorCart.Slides)
            {
                s.UpdateGlobal(GlobalData);
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
            catch (Exception)
            {
                // TODO Log
            }
        }

        #region ImageGeneration
        private Dictionary<string, ManagerImageReference> callBackRefs = new Dictionary<string, ManagerImageReference>();
        public void DispatchGeneration(string id, long serial, ManagerImageReference image)
        {
            var task = new WorkerTaskMessage();
            task.Type = "io.ebu.eis.task.generateimage";
            task.Id = id;
            task.Serial = serial;
            task.GenerationProps = Config.ImageGemerationConfiguration.GenerationProperties;
            task.ImageReference = image;

            var json = JsonSerializer.Serialize(task);
            _imageGeneratorMq.Dispatch(json);
        }

        public void RegisterImageCallback(string id, long serial, ManagerImageReference image)
        {
            if (!callBackRefs.ContainsKey(id))
            {
                callBackRefs.Add(id, image);
            }
            else
            {
                // TODO log should not happen;
            }
        }
        public bool HandleWorkerTask(string js)
        {
            try
            {
                var task = JsonSerializer.Deserialize<WorkerTaskMessage>(js);

                switch (task.Type)
                {
                    case "io.ebu.eis.image":
                        {
                            // Find image reference
                            lock (callBackRefs)
                            {
                                if (callBackRefs.ContainsKey(task.Id))
                                {
                                    var img = callBackRefs[task.Id];
                                    lock (img)
                                    {
                                        if (task.Serial > img.LastUpdateSerial)
                                        {
                                            // We update since new generation is newer
                                            img.DispatchedExternalPreviewUpdate(task.Serial, task.Base64ImageData);
                                        }
                                    }
                                    if (img.LastUpdateSerial == img.ExpectedSerial)
                                    {
                                        // No more updates expected, thus remove from callback
                                        callBackRefs.Remove(task.Id);
                                    }

                                }
                                else
                                {
                                    // TODO
                                }
                            }

                        }
                        break;
                    default:
                        Console.WriteLine($"Uknown task of type {task.Type}");
                        break;
                }

                // Finally return true
                return true;
            }
            catch (Exception ex)
            {
                // TODO log
            }
            return false;
        }


        #endregion ImageGeneration

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
