using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using io.ebu.eis.datastructures;
using io.ebu.eis.mq;
using io.ebu.eis.shared;

namespace io.ebu.eis.worker
{

    public class WorkerContext : INotifyPropertyChanged, IDataMessageHandler, IDisposable
    {
        private readonly CMConfigurationSection _config;
        public CMConfigurationSection Config { get { return _config; } }

        private readonly List<AMQQueuePublisher> _publishers = new List<AMQQueuePublisher>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private string _status = "Starting...";
        public string Status { get { return _status; } set { _status = value; OnPropertyChanged("Status"); } }

        public WorkerContext()
        {
            _config = (CMConfigurationSection)ConfigurationManager.GetSection("CMConfiguration");

            #region INPUTS
            // Open Connection to INBOUND MQ
            foreach (InputConfiguration input in _config.InputConfigurations)
            {
                switch (input.Type.ToUpper())
                {
                    case "MQ":
                        {
                            var amquri = input.MQUri;
                            var amqinQueue = input.MQQueue;
                            var dataInConnection = new AMQQueueConsumer(amquri, amqinQueue, this);
                            dataInConnection.ConnectAsync();
                            // TODO Catch hand handle connection exceptions and reconnect

                            _disposables.Add(dataInConnection);
                        }
                        break;
                        //case "HTTP":
                        //    {
                        //        var httpBindIp = input.BindIp;
                        //        var httpBindPort = input.BindPort;
                        //        var dataInHttpServer = new CMHttpServer(httpBindIp, httpBindPort, this);
                        //        dataInHttpServer.Start();

                        //        _disposables.Add(dataInHttpServer);
                        //    }
                        //    break;
                        // TODO Handle and log default
                }
            }

            #endregion INPUTS

            #region OUTPUTS
            // Open Connection to OUTBOUND MQ if enabled and URI defined
            var amqDispatchUri = _config.OutputConfiguration.DispatchMQConfiguration.MQUri;
            if (_config.OutputConfiguration.EnableDataDispatchMQ && !string.IsNullOrEmpty(amqDispatchUri))
            {
                var amqoutqueue = _config.OutputConfiguration.DispatchMQConfiguration.MQQueue;
                var dataOutConnection = new AMQQueuePublisher(amqDispatchUri, amqoutqueue);
                dataOutConnection.ConnectAsync();
                _publishers.Add(dataOutConnection);
                // TODO Catch hand handle connection exceptions and reconnect

                _disposables.Add(dataOutConnection);
            }
            #endregion OUTPUTS

        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }

        #region TaskHandling

        public bool HandleWorkerTask(string js)
        {
            try
            {
                var task = JsonSerializer.Deserialize<WorkerTaskMessage>(js);

                switch (task.Type)
                {
                    case "io.ebu.eis.task.generateimage":
                        {
                            // Set Config in ImageRef
                            task.ImageReference.Config = Config;
                            var img = task.ImageReference.RenderAndReturnBase64(task.GenerationProps);

                            // Generate reply
                            var reply = new WorkerTaskMessage();
                            reply.Type = "io.ebu.eis.image";
                            reply.Id = task.Id;
                            reply.Serial = task.Serial;
                            reply.Base64ImageData = img;

                            var jsReply = JsonSerializer.Serialize(reply);
                            // Dispatch Result back to RMQ
                            foreach (var p in _publishers)
                            {
                                p.Dispatch(jsReply);
                            }

                            // Set local preview
                            MainImage = task.ImageReference;
                        }
                        break;
                    default:
                        Status = $"Uknown task of type {task.Type}";
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
        #endregion TaskHandling

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

        public void OnReceive(DataMessage message)
        {
            // Do studd here
        }

        public void UpdateGlobalData(DataMessage message)
        {
            throw new NotImplementedException();
        }

        public void BroadcastSlide(string slidename)
        {
            throw new NotImplementedException();
        }

        public void ClearActiveCart()
        {
            throw new NotImplementedException();
        }

        public void AddSlides(List<string> slideNames)
        {
            throw new NotImplementedException();
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
