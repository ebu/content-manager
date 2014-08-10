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
            InitializeComponent();

            _context = (ManagerContext)DataContext;
            
            //Enable the cross acces to this collection elsewhere
            BindingOperations.EnableCollectionSynchronization(_context.Carts, _synLock);


            // Start Processes
            _running = true;
            Thread t1 = new Thread(AutoProcessing);
            t1.Start();
        }

        DateTime lastAutomationChange = DateTime.MinValue;
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

                        // Automation Mode and Slide change
                        if (_context.InAutomationMode)
                        {
                            if (lastAutomationChange.AddSeconds(_context.AutomationInterval) < DateTime.Now)
                            {
                                // Switch to next slide if in Automation
                                _context.SwitchToNextSlide();
                                lastAutomationChange = DateTime.Now;
                                _context.AutomationProgress = 1.0;
                            }

                            // Update internal values
                            var millisToNextChange = Math.Max(0, lastAutomationChange.AddSeconds(_context.AutomationInterval).Subtract(DateTime.Now).TotalMilliseconds);
                            _context.AutomationProgress = (0.0 + millisToNextChange) / (_context.AutomationInterval * 1000);

                        }
                        else
                        {
                            _context.AutomationProgress = 0.0;
                        }

                        DataContext = _context;
                    }, null);


                    Monitor.Wait(_synLock, 100);
                }
            }
        }

        private ManagerImageReference __lastSelectedManagerImageRef;
        private void PreviewListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ManagerImageReference)
            {
                __lastSelectedManagerImageRef = e.AddedItems[0] as ManagerImageReference;
            }

        }

        private void PreviewListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                // Selected other Slide to be pushed !
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (SendOrPostCallback)delegate
                {
                    _context = (ManagerContext)DataContext;
                    if (__lastSelectedManagerImageRef != null)
                    {
                        lastAutomationChange = DateTime.Now;
                        _context.SwitchToSlide(__lastSelectedManagerImageRef);
                    }

                }, null);
            }
            if (e.ClickCount == 1 && e.RightButton == MouseButtonState.Pressed)
            {
                // Right mouse click, to edit
                _context = (ManagerContext)DataContext;
                if (__lastSelectedManagerImageRef != null)
                {
                    _context.EditorImage = __lastSelectedManagerImageRef;
                }

            }
        }


        private void mainImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Copy public URL to Clipboard
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (SendOrPostCallback)delegate
                {
                    _context = (ManagerContext)DataContext;
                    if (_context.MainImage != null)
                        System.Windows.Clipboard.SetText(_context.MainImage.PublicImageUrl);

                }, null);
            }
        }


        private void PreviewListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Remove the last selected Slide from the active Cart
            if (e.Key == Key.Delete)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (SendOrPostCallback)delegate
                {
                    _context = (ManagerContext)DataContext;
                    if (__lastSelectedManagerImageRef != null)
                    {
                        if (_context.ActiveCart.Slides.Contains(__lastSelectedManagerImageRef))
                        {
                            _context.ActiveCart.Slides.Remove(__lastSelectedManagerImageRef);
                            _context.ReloadPreview();
                        }
                    }

                }, null);
            }
        }


        private void EditorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selected other Slide to be pushed !
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
            (SendOrPostCallback)delegate
            {
                _context = (ManagerContext)DataContext;
                if (e.AddedItems.Count > 0 && e.AddedItems[0] is ManagerImageReference)
                {
                    var newImage = e.AddedItems[0] as ManagerImageReference;
                    _context.EditorImage = newImage.Clone();
                }

            }, null);
        }


        private void rendreButtonTest_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
            (SendOrPostCallback)delegate
            {
                _context = (ManagerContext)DataContext;
                if (_context.EditorImage != null)
                {
                    _context.EditorImage.Render();
                    _context.EditorImage = _context.EditorImage;
                }

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

                        var cartName = "";
                        switch (m.DataMessage.DataType)
                        {
                            case "WEATHER": cartName = "WEATHER"; break;
                            case "STARTLIST": cartName = "STARTLIST"; break;
                            case "RESULTLIST": cartName = "RESULTLIST"; break;
                        }

                        _context = (ManagerContext)DataContext;

                        var existingCart = _context.Carts.FirstOrDefault(x => x.Name.ToUpper() == cartName);
                        if (existingCart != null)
                        {
                            // TODO ASK IF Switch Cart or add to cart

                            var newCart = existingCart.Clone();
                            newCart.Name = newCart.Name + " - " + m.DataMessage.GetValue("EVENTNAME");
                            if (newCart.Slides.Count == 1 && newCart.Slides.First().CanRepeate)
                            {
                                // We might need to repeate the slide due to data and slit the context
                                // We assume 4 line per slide
                                // TODO This needs to be generic !
                                var context = m.DataMessage.Clone();

                                var ath = context.Data.FirstOrDefault(x => x.Key == "ATHLETES");
                                newCart.Slides.First().Context = context;
                                var offset = 4;
                                while (ath.Data.Count > 4)
                                {
                                    var clone = ath.Clone();
                                    clone.Data.RemoveRange(0, 4);
                                    ath.Data.RemoveRange(4, ath.Data.Count - 4);
                                    var newSlide = newCart.Slides.First().Clone();
                                    newSlide.IndexOffset = offset;
                                    var contextCLone = context.Clone();
                                    contextCLone.Data.First(x => x.Key == "ATHLETES").Data = clone.Data;
                                    newSlide.Context = contextCLone;
                                    newCart.Slides.Add(newSlide);
                                    ath = clone;
                                    offset = offset + 4;
                                }
                            }
                            else
                            {
                                foreach (var s in newCart.Slides)
                                {
                                    // Set the context on all slides
                                    s.Context = m.DataMessage;
                                }
                            }

                            _context.Carts.Add(newCart);
                            _context.ActiveCart = newCart;
                            _context.ReloadPreview();
                        }
                        else
                        {
                            // TODO if no cart the apply to editor template as context
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO HAndle and not generic Exceltion
            }
        }


        private void ImageFlowList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ListBox)
                {
                    var box = sender as ListBox;
                    if (box.SelectedItem is DataFlowItem)
                    {
                        var m = box.SelectedItem as DataFlowItem;
                        _context.EditorImage.Background = m.Url;
                        _context.EditorImage.Render();
                        _context.EditorImage = _context.EditorImage;

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
            _context = (ManagerContext)DataContext;

            _running = false;
            lock (_synLock)
            {
                Monitor.PulseAll(_synLock);
            }
            _context.Stop();
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
           (SendOrPostCallback)delegate
           {
               _context = (ManagerContext)DataContext;
               _running = false;
               _context.Stop();

           }, null);
        }

        private void editorAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Add the image to the current Active Cart
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
           (SendOrPostCallback)delegate
           {
               _context = (ManagerContext)DataContext;
               _context.ActiveCart.Slides.Add(_context.EditorImage.Clone());
               _context.ReloadPreview();

           }, null);
        }

        private void editorAddButtonAndMain_Click(object sender, RoutedEventArgs e)
        {
            // Add the image to the current Active Cart and set it active
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
           (SendOrPostCallback)delegate
           {
               _context = (ManagerContext)DataContext;
               var newImg = _context.EditorImage.Clone();
               _context.ActiveCart.Slides.Add(newImg);
               _context.ReloadPreview();
               lastAutomationChange = DateTime.Now;
               _context.SwitchToSlide(newImg);

           }, null);
        }

        private void ImageFlowList_Drop(object sender, DragEventArgs e)
        {

            Dispatcher.BeginInvoke(DispatcherPriority.Render,
          (SendOrPostCallback)delegate
          {
              _context = (ManagerContext)DataContext;
              var data = e.Data;
              if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
              {
                  string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                  foreach (var path in droppedFilePaths)
                  {
                      _context.IngestImage(path);
                  }
              }

          }, null);

        }






    }
}
