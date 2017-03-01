using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using io.ebu.eis.mq;

namespace io.ebu.eis.worker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WorkerContext _context;
        private bool _running;
        private readonly object _synLock = new object();

        public MainWindow()
        {
            //Global Exception Handling
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += GlobalExceptionHandler;

            InitializeComponent();

            // Window Title
            var currentAssembly = Assembly.GetExecutingAssembly();
            string versionNumber = currentAssembly.GetName().Version.ToString();
            Title = Title + " (" + versionNumber + ")";

            _context = (WorkerContext) DataContext;
            _context.Status = "Starting up...";
            //Enable the cross acces to this collection elsewhere
            //BindingOperations.EnableCollectionSynchronization(_context.Object, _synLock);
            
            // Start Processes
            _running = true;
            var t1 = new Thread(AutoProcessing);
            t1.Start();

            _context.Status = "Started.";
        }

        #region GlobalExceptions

        // Global Exception Handler called when a uncaught Exception is thrown
        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            //RavenClient ravenClient = new RavenClient("url");
            //ravenClient.CaptureException(e, "Global Exception in MainWindow WFS", SharpRaven.Data.ErrorLevel.Fatal);
            //Reporter.Exception(e);

            // TODO Write to log

            var e = (Exception) args.ExceptionObject;
            MessageBox.Show("Global Exception Handler caught : \n\n" + e.Message + "\n\n" + e.StackTrace);
        }

        #endregion GlobalExceptions

        private void AutoProcessing()
        {
            while (_running)
            {
                lock (_synLock)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Render,
                        (SendOrPostCallback) delegate
                        {
                            _context = (WorkerContext) DataContext;

                            // Do Stuff
                        }, null);


                    Monitor.Wait(_synLock, 100);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _running = false;
        }
    }
}
