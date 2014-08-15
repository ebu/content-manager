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

                            // Update internal values progresses
                            var millisToNextChange = Math.Max(0, lastAutomationChange.AddSeconds(_context.AutomationInterval).Subtract(DateTime.Now).TotalMilliseconds);
                            _context.AutomationProgress = (0.0 + millisToNextChange) / (_context.AutomationInterval * 1000);

                            if (_context.IsInOverrideCart)
                            {
                                var millisToOverride = Math.Max(0,
                                    lastAutomationChange.AddSeconds(_context.AutomationInterval)
                                        .Subtract(DateTime.Now)
                                        .TotalMilliseconds);
                                _context.OverrideProgress = (0.0 + millisToNextChange +
                                                             1000 * _context.OverrideSlideCountDown * _context.AutomationInterval) /
                                                            (_context.OverrideRotationCount *
                                                             _context.ActiveCart.Slides.Count *
                                                             _context.AutomationInterval * 1000);
                            }
                        }
                        else
                        {
                            _context.AutomationProgress = 0.0;
                        }

                        // Override Mode and Back to INITIAL Cart
                        if (_context.IsInOverrideCart && _context.OverrideSlideCountDown == 0)
                        {
                            var initCart = _context.Carts.FirstOrDefault(x => x.Name == "INITIAL");
                            if (initCart != null)
                            {
                                _context.ActiveCart = initCart;
                                _context.IsInOverrideCart = false;
                                _context.ReloadPreview();
                            }
                            _context.OverrideProgress = 0.0;
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
                        if (_context.PreviewCart.Slides.Contains(__lastSelectedManagerImageRef) && _context.PreviewCart.Slides.Count > 1)
                        {
                            _context.PreviewCart.Slides.Remove(__lastSelectedManagerImageRef);
                            _context.ReloadPreview();
                        }
                    }

                }, null);
            }
        }

        private ManagerImageReference __lastSelectedEditorImageRef;
        private void EditorListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Remove the last selected Slide from the active Cart
            if (e.Key == Key.Delete)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (SendOrPostCallback)delegate
                {
                    _context = (ManagerContext)DataContext;
                    if (__lastSelectedEditorImageRef != null)
                    {
                        if (_context.EditorCart.Slides.Contains(__lastSelectedEditorImageRef) && _context.EditorCart.Slides.Count > 1)
                        {
                            _context.EditorCart.Slides.Remove(__lastSelectedEditorImageRef);
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
                    __lastSelectedEditorImageRef = e.AddedItems[0] as ManagerImageReference;
                    _context.EditorImage = __lastSelectedEditorImageRef.Clone();
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

                        _context.CreateCartForDataFlowItem(m);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO HAndle and not generic Exceltion
            }
        }

        private void DataFlowList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Reset the preview cart to the active cart
                _context = (ManagerContext)DataContext;
                if (_context.ActiveCart != _context.PreviewCart)
                {
                    _context.PreviewCart = _context.ActiveCart;
                }
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

        #region CartSelection

        private ManagerCart _lastSelectedCart;
        private void CartListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ListBox)
                {
                    var box = sender as ListBox;
                    if (box.SelectedItem is ManagerCart)
                    {
                        var selectedCart = box.SelectedItem as ManagerCart;
                        _lastSelectedCart = selectedCart;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO HAndle and not generic Exceltion
            }
        }

        private void CartListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                // Selected other Slide to be pushed !
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (SendOrPostCallback)delegate
                {
                    _context = (ManagerContext)DataContext;
                    if (_lastSelectedCart != null)
                    {
                        _context.PreviewCart = _lastSelectedCart;
                        _context.ReloadPreview();
                    }

                }, null);

                e.Handled = true;
            }
        }


        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Remove the last selected Cart from the list
            if (e.Key == Key.Delete)
            {
                _context = (ManagerContext)DataContext;
                if (_lastSelectedCart != null)
                {
                    if (_context.Carts.Contains(_lastSelectedCart))
                    {
                        _context.Carts.Remove(_lastSelectedCart);
                    }
                }

            }
        }

        #endregion CartSelection

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _context = (ManagerContext)DataContext;

            _running = false;
            lock (_synLock)
            {
                Monitor.PulseAll(_synLock);
            }
            _context.Stop();
            _context.SerializeToFile();
        }

        private void editorAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Add the image to the current Active Cart
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
           (SendOrPostCallback)delegate
           {
               _context = (ManagerContext)DataContext;
               _context.PreviewCart.Slides.Add(_context.EditorImage.Clone());
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
               _context.PreviewCart.Slides.Add(newImg);
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


        private void TemplateListBox_Drop(object sender, DragEventArgs e)
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
                      if (path.EndsWith(".html"))
                      {
                          // Add the template as slide to the Templates list
                          _context.AddEditorTemplate(path);
                      }
                  }
              }

          }, null);
        }


        private void TemplateListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Does not work 'cause SelectionChanged happens after mouse down
          //  Dispatcher.BeginInvoke(DispatcherPriority.Render,
          //(SendOrPostCallback)delegate
          //{
          //    _context = (ManagerContext)DataContext;

          //    if (_context.EditorImage.Template != __lastSelectedEditorImageRef.Template)
          //    {
          //        // Add the template as slide to the Templates list
          //        _context.EditorImage = __lastSelectedEditorImageRef.Clone();
          //    }
          //}, null);
        }

        private void cancelPreviewCartButton_Click(object sender, RoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;
            _context.PreviewCart = _context.ActiveCart;
            _context.ReloadPreview();
        }

        private void onAirPreviewCartButton_Click(object sender, RoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;
            if (_context.PreviewCart.Slides.Count > 0)
            {
                _context.ActiveCart = _context.PreviewCart;
                _context.ReloadPreview();
            }
            else
            {
                // TODO Error
            }
        }


        private void addPreviewCartButton_Click(object sender, RoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;
            if (_context.PreviewCart.Slides.Count > 0)
            {
                foreach (var s in _context.PreviewCart.Slides)
                {
                    _context.ActiveCart.Slides.Add(s);
                }
                _context.PreviewCart = _context.ActiveCart;
                _context.ReloadPreview();
            }
            else
            {
                // TODO Error
            }
        }

        private void clearAllCartsButton_Click(object sender, RoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;
            while (_context.Carts.Count(x => x.CanBeDeleted && x != _context.ActiveCart && x != _context.PreviewCart) > 0)
            {
                var todelCart = _context.Carts.FirstOrDefault(x => x.CanBeDeleted && x != _context.ActiveCart && x != _context.PreviewCart);
                if (todelCart != null)
                {
                    _context.Carts.Remove(todelCart);
                }
            }
        }

        private void newCartButton_Click(object sender, RoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;
            var c = new ManagerCart("TEMP CART");
            c.CanBeDeleted = true;
            c.ShowInCartList = true;

            _context.Carts.Add(c);
            _context.PreviewCart = c;

        }

        private void ClearImageFlowFilter_Click(object sender, RoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;
            _context.ImageFlowFilterString = "";
        }

        private void ClearDataFlowFilter_Click(object sender, RoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;
            _context.DataFlowFilterString = "";
        }













    }
}
