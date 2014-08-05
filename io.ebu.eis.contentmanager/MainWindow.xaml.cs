using io.ebu.eis.canvasgenerator;
using io.ebu.eis.data.s3;
using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;
using io.ebu.eis.mq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class MainWindow : Window
    {
        private CMConfigurationSection _config;
        private ManagerContext _context;
        private bool _running;
        private object _synLock = new object();

        
        public MainWindow()
        {
            // Data Context
            _context = new ManagerContext();
            DataContext = _context;



            InitializeComponent();

            //Enable the cross acces to this collection elsewhere
            BindingOperations.EnableCollectionSynchronization(_context.Carts, _synLock);


            // Start Processes
            _running = true;
            Thread t1 = new Thread(AutoProcessing);
            t1.Start();
        }


        private void AutoProcessing()
        {
            while (_running)
            {
                lock (_synLock)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Render,
                    (SendOrPostCallback)delegate
                    {
                        _context = (ManagerContext)DataContext;

                        if (_context.InAutomationMode)
                        {
                            // Switch to next slide if in Automation
                            _context.SwitchToNextSlide();
                        }
                    }, null);

                    Monitor.Wait(_synLock, _context.AutomationInterval * 1000);
                }
            }
        }


        private void rendreButtonTest_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
            (SendOrPostCallback)delegate
            {
                _context = (ManagerContext)DataContext;
                // TODO
                //_context.MainImage = _renderer.Render("zurichsample001.html");
                var i = new ManagerImageReference(_context.Renderer, _config)
                {
                    Template = System.IO.Path.Combine(_config.SlidesConfiguration.TemplatePath, "datesample03.html")
                };
                _context.MainImage = i;

            }, null);

        }

        private void DataFlowList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ListBox)
                {
                    var box = sender as ListBox;
                    if (box.SelectedItem is DataFlowItem)
                    {
                        var m = box.SelectedItem as DataFlowItem;
                        var template = "index.html";
                        switch (m.DataMessage.DataType)
                        {
                            case "WEATHER": template = "weather.html"; break;
                            case "STARTLIST": template = "startlist.html"; break;
                        }
                        var i = new ManagerImageReference(_context.Renderer, _config)
                        {
                            Template = template,
                            Context = m.DataMessage
                        };
                        _context.PreviewImage = i;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO HAndle and not generic Exceltion
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
           (SendOrPostCallback)delegate
           {
               _context = (ManagerContext)DataContext;


               _running = false;
               _context.Stop();

           }, null);
        }

    }
}
