using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;
using io.ebu.eis.mq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace io.ebu.eis.contentmanager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IAMQDataMessageHandler
    {
        CMConfigurationSection _config;
        ManagerContext _context;

        AMQConsumer _dataInConnection;

        public MainWindow()
        {
            _config = (CMConfigurationSection)ConfigurationManager.GetSection("CMConfiguration");

            // Data Context
            _context = new ManagerContext();
            DataContext = _context;

            InitializeComponent();
            
            //_context.DummyData();

            // Open Connection to INBOUND MQ
            var amquri = _config.MQConfiguration.Uri;
            var amqexchange = _config.MQConfiguration.DPExchange;
            _dataInConnection = new AMQConsumer(amquri, amqexchange, this);
            _dataInConnection.Connect();

        }

        public void OnReceive(DataMessage message)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
            (SendOrPostCallback)delegate
            {
                _context = (ManagerContext)DataContext;

                if (_config.DataConfiguration.DataFlowTypes.Split(';').Contains(message.DataType))
                {
                    // Add the message to the data flow
                    DataFlowItem d = new DataFlowItem()
                    {
                        Name = message.Value,
                        Category = message.Key,
                        Type = message.DataType,
                        Short = message.Data.ToString(),
                        Priority = DataFlowPriority.Low
                    };

                    _context.DataFlowItems.Add(d);
                }
                if (_config.DataConfiguration.ImageFlowTypes.Split(';').Contains(message.DataType))
                {
                    // Add the message to the image flow
                    DataFlowItem d = new DataFlowItem()
                    {
                        Name = message.Value,
                        Category = message.Key,
                        Type = message.DataType,
                        Short = message.Data.ToString(),
                        Priority = DataFlowPriority.Low
                    };

                    _context.ImageFlowItems.Add(d);
                }

                if (_config.DataConfiguration.DataBaseTypes.Split(';').Contains(message.DataType))
                {
                    // Add the message to the database
                    _context.UpdateDataBase(message);
                }

            }, null);
            
        }
    }
}
