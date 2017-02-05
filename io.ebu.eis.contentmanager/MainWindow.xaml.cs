using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using io.ebu.eis.data.file;
using io.ebu.eis.datastructures;
using io.ebu.eis.shared;
using io.ebu.eis.shared.Utils;
using Microsoft.Win32;

namespace io.ebu.eis.contentmanager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ManagerContext _context;
        private bool _running;
        private readonly object _synLock1 = new object();
        private readonly object _synLock2 = new object();
        private readonly object _synLock3 = new object();

        private DateTime _lastAutomationChange = DateTime.MinValue;
        private ManagerImageReference _lastSelectedManagerImageRef;
        private ManagerImageReference _lastSelectedEditorImageRef;
        private ManagerCart _lastSelectedCart;

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

            _context = (ManagerContext)DataContext;

            //Enable the cross acces to this collection elsewhere
            BindingOperations.EnableCollectionSynchronization(_context.Carts, _synLock1);
            BindingOperations.EnableCollectionSynchronization(_context.ImageFlowItems, _synLock2);
            BindingOperations.EnableCollectionSynchronization(_context.DataFlowItems, _synLock3);


            // Start Processes
            _running = true;
            var t1 = new Thread(AutoProcessing);
            t1.Start();


            // Standalone no DataItemConfigurations Settings
            if (_context?.Config?.DataConfiguration?.DataItemConfigurations.Count == 0)
            {
                // Column 4 is the data column - would be changed back if any layout stored
                HorizontalSystemGrid.ColumnDefinitions[4].Width = new GridLength(0.0);
                // Also hide Data Buttons and Bar
                autoDataButton.Visibility = Visibility.Hidden;
                dataOverrideInterval.Visibility = Visibility.Hidden;
                dataOverrideProgress.Visibility = Visibility.Hidden;

            }

            // Start Watchfolder if incoming picture defined
            if (!string.IsNullOrEmpty(_context?.Config?.DataConfiguration?.IncomingPictureFolder))
            {
                _imageWatchFolder = new SystemWatchfolder(_context.Config.DataConfiguration.IncomingPictureFolder,
                    "*.jpg|*.png|*.gif", false, _context);
                _imageWatchFolder.Start();
            }

            // Restore
            RestoreLayout();
        }

        private SystemWatchfolder _imageWatchFolder;

        #region GlobalExceptions
        // Global Exception Handler called when a uncaught Exception is thrown
        static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            //RavenClient ravenClient = new RavenClient("url");
            //ravenClient.CaptureException(e, "Global Exception in MainWindow WFS", SharpRaven.Data.ErrorLevel.Fatal);
            //Reporter.Exception(e);

            // TODO Write to log

            var e = (Exception)args.ExceptionObject;
            MessageBox.Show("Global Exception Handler caught : \n\n" + e.Message + "\n\n" + e.StackTrace);
        }
        #endregion GlobalExceptions


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Stop Watchfolder
            _imageWatchFolder?.Stop();

            _context = (ManagerContext)DataContext;

            _running = false;
            lock (_synLock1)
            {
                Monitor.PulseAll(_synLock1);
            }
            _context.Dispose();
            try
            {
                _context.SerializeToFile();
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry($"EIS ContentManager was unable to store the configuration to file.\n{ex.Message}\n\n{ex.StackTrace}",
                        EventLogEntryType.Error, 101, 1);
                }
            }
        }


        #region AutoProcessing

        private void AutoProcessing()
        {
            while (_running)
            {
                lock (_synLock1)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Render,
                    (SendOrPostCallback)delegate
                    {
                        _context = (ManagerContext)DataContext;

                        // Automation Mode and Slide change
                        if (_context.InAutomationMode)
                        {
                            if (_lastAutomationChange.AddSeconds(_context.AutomationInterval) < DateTime.Now)
                            {
                                // Switch to next slide if in Automation
                                _context.SwitchToNextSlide();
                                _lastAutomationChange = DateTime.Now;
                                _context.AutomationProgress = 1.0;
                            }

                            // Update internal values progresses
                            var millisToNextChange = Math.Max(0, _lastAutomationChange.AddSeconds(_context.AutomationInterval).Subtract(DateTime.Now).TotalMilliseconds);
                            _context.AutomationProgress = (0.0 + millisToNextChange) / (_context.AutomationInterval * 1000);

                            if (_context.IsInOverrideCart)
                            {
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


                    Monitor.Wait(_synLock1, 100);
                }
            }
        }

        #endregion AutoProcessing



        #region OnAirImageManagement

        private void mainImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Copy public URL to Clipboard
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (SendOrPostCallback)delegate
                {
                    try
                    {
                        _context = (ManagerContext)DataContext;
                        if (_context.MainImage != null)
                        {
                            var text = string.Join("\n", _context.MainImage.ImageVariants.Select(i => $"{i.Name} : {i.Url}"));
                            Clipboard.SetText(text);
                        }
                    }
                    catch (Exception)
                    {
                        // TODO Log 
                    }
                }, null);
            }
        }

        #endregion OnAirImageManagement

        #region EditorFunctions

        private void rendreButtonTest_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
            (SendOrPostCallback)delegate
            {
                _context = (ManagerContext)DataContext;
                if (_context.EditorImage != null)
                {
                    _context.EditorImage.ReRender(true);
                    _context.EditorImage = _context.EditorImage;
                }

            }, null);

        }

        private void ClearTextField_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var btn = button;
                var context = btn.DataContext;
                if (!(context is ManagerTemplateField)) return;
                var field = context as ManagerTemplateField;
                field.Value = "";
            }
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
               if (_context.EditorImage != null)
               {
                   _context = (ManagerContext)DataContext;
                   var newImg = _context.EditorImage.Clone();
                   _context.PreviewCart.Slides.Add(newImg);
                   _context.ReloadPreview();
                   _lastAutomationChange = DateTime.Now;
                   _context.SwitchToSlide(newImg);
               }
           }, null);
        }

        private void EditorListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Remove the last selected Slide from the active Cart
            if (e.Key == Key.Delete)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (SendOrPostCallback)delegate
                {
                    _context = (ManagerContext)DataContext;
                    if (_lastSelectedEditorImageRef != null)
                    {
                        if (_context.EditorCart.Slides.Contains(_lastSelectedEditorImageRef) && _context.EditorCart.Slides.Count > 1)
                        {
                            _context.EditorCart.Slides.Remove(_lastSelectedEditorImageRef);
                            _context.ReloadPreview();
                        }
                    }

                }, null);
            }
        }


        #endregion EditorFunctions


        private void CartListBox_Drop(object sender, DragEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
          (SendOrPostCallback)delegate
          {
              _context = (ManagerContext)DataContext;
              if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
              {
                  string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                  if (droppedFilePaths != null)
                      foreach (var path in droppedFilePaths)
                      {
                          if (path.EndsWith(".cartjson"))
                          {
                              var json = File.ReadAllText(path);
                              try
                              {
                                  var newCart = JsonSerializer.Deserialize<ManagerCart>(json);
                                  newCart.CanBeDeleted = true;
                                  newCart.ShowInCartList = true;
                                  newCart.IsActive = false;
                                  foreach (var s in newCart.Slides)
                                      s.Config = _context.Config;
                                  _context.Carts.Add(newCart);
                              }
                              catch (Exception)
                              {
                                  // TODO LOG
                              }
                          }
                      }
              }
          }, null);
        }

        #region ContextMenus
        public void CtxMenu_SaveCart(object sender, ExecutedRoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;

            var item = e.OriginalSource as ListBoxItem;
            if (item != null)
            {
                var lbi = item;

                var managerCart = lbi.Content as ManagerCart;
                if (managerCart != null)
                {
                    var cart = managerCart;

                    SaveFileDialog dlg = new SaveFileDialog
                    {
                        FileName = cart.Name,
                        DefaultExt = ".cartjson",
                        Filter = "Cart JSON File (.cartjson)|*.cartjson"
                    };
                    bool? result = dlg.ShowDialog();
                    if (result == true)
                    {
                        string filename = dlg.FileName;
                        var json = JsonSerializer.Serialize(cart);
                        File.WriteAllText(filename, json);
                    }
                }
            }
        }
        #endregion ContextMenus

        #region DataFLowManagement

        private void DataFlowList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            //    if (sender is ListBox)
            //    {
            //        var box = sender as ListBox;
            //        if (box.SelectedItem is DataFlowItem)
            //        {
            //            var m = box.SelectedItem as DataFlowItem;

            //            _context.CreateCartForDataFlowItem(m, false);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // TODO HAndle and not generic Exceltion
            //}
        }

        private void DataFlowList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var grid1 = sender as Grid;
                if (grid1 != null)
                {
                    var grid = grid1;
                    var item = grid.DataContext as DataFlowItem;
                    if (item != null)
                    {
                        var m = item;

                        //if (_context.Config.DataConfiguration.AutoCartClearTypes.Split(';').Contains(m.DataMessage.DataType))
                        //{
                        //    // Need to clear the current active cart
                        //    _context.ActiveCart.Slides.Clear();
                        //}
                        if (_context.Config.DataConfiguration.AutoCartAppendTypes.Split(';').Contains(m.DataMessage.DataType))
                        {
                            // Need to append to current active cart
                            _context.CreateCartForDataFlowItem(m, _context.PreviewCart);
                        }
                        if (_context.Config.DataConfiguration.AutoCartGenerationTypes.Split(';').Contains(m.DataMessage.DataType))
                        {
                            // Need to create a new cart
                            _context.CreateCartForDataFlowItem(m, null);
                        }
                        if (_context.Config.DataConfiguration.AutoEditorTypes.Split(';').Contains(m.DataMessage.DataType))
                        {
                            // Need to update the editor's template
                            if (_context.Config.DataConfiguration.DataFlowLeftActions.Split(';').Contains("Template"))
                            {
                                // Load template to editor
                                // Find appropriate template
                                var conf = _context.Config.DataConfiguration.DataItemConfigurations.Cast<DataItemConfiguration>().FirstOrDefault(c => c.DataType == m.DataMessage.DataType);
                                // Load corresponding template
                                if (conf != null)
                                {
                                    var newTemplate = new ManagerImageReference(_context.Config, _context)
                                    {
                                        Template = conf.DefaultTemplate,
                                        Link = _context.Config.SlidesConfiguration.DefaultLink,
                                        CanRepeate = false
                                    };

                                    _context.EditorImage = newTemplate;
                                }
                            }
                            if (_context.Config.DataConfiguration.DataFlowLeftActions.Split(';').Contains("BackgroundUrl"))
                            {
                                // Set the background Uri
                                _context.EditorImage.Background = m.Url;
                            }
                            if (_context.Config.DataConfiguration.DataFlowLeftActions.Split(';').Contains("Context"))
                            {
                                // Set the context
                                _context.EditorImage.Context = m.DataMessage;
                            }
                            _context.EditorImage.ReRender(true);
                            _context.EditorImage = _context.EditorImage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO HAndle and not generic Exceltion
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry($"EIS Content Manager unable to handle MouseLeftButton Down on data.\n{ex.Message}\n\n{ex.StackTrace}", EventLogEntryType.Error, 101, 1);
                }
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

        private void ClearDataFlowFilter_Click(object sender, RoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;
            _context.DataFlowFilterString = "";
        }

        #endregion DataFLowManagement

        #region ImageFLowManagement

        private void ImageFlowList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ImageFlowList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var grid = sender as Grid;
                if (grid != null)
                {
                    var item = grid.DataContext as DataFlowItem;
                    if (item != null)
                    {
                        var m = item;

                        //if (_context.Config.DataConfiguration.AutoCartClearTypes.Split(';').Contains(m.DataMessage.DataType))
                        //{
                        //    // Need to clear the current active cart
                        //    _context.ActiveCart.Slides.Clear();
                        //}
                        if (_context.Config.DataConfiguration.AutoCartAppendTypes.Split(';').Contains(m.DataMessage.DataType))
                        {
                            // Need to append to current active cart
                            _context.CreateCartForDataFlowItem(m, _context.PreviewCart);
                        }
                        if (_context.Config.DataConfiguration.AutoCartGenerationTypes.Split(';').Contains(m.DataMessage.DataType))
                        {
                            // Need to create a new cart
                            _context.CreateCartForDataFlowItem(m, null);
                        }
                        if (_context.Config.DataConfiguration.AutoEditorTypes.Split(';').Contains(m.DataMessage.DataType))
                        {
                            // Need to update the editor's template
                            if (_context.Config.DataConfiguration.ImageFlowLeftActions.Split(';').Contains("Template"))
                            {
                                // Load template to editor
                                // Find appropriate template
                                var conf = _context.Config.DataConfiguration.DataItemConfigurations.Cast<DataItemConfiguration>().FirstOrDefault(c => c.DataType == m.DataMessage.DataType);
                                // Load corresponding template
                                if (conf != null)
                                {
                                    var newTemplate = new ManagerImageReference(_context.Config, _context)
                                    {
                                        Template = conf.DefaultTemplate,
                                        Link = _context.Config.SlidesConfiguration.DefaultLink,
                                        CanRepeate = false
                                    };
                                    // Set the background Uri right away so no regeneration required
                                    if (_context.Config.DataConfiguration.ImageFlowLeftActions.Split(';').Contains("BackgroundUrl"))
                                    {
                                        newTemplate.Background = m.Url;
                                    }

                                    _context.EditorImage = newTemplate;
                                }
                            }
                            if (_context.Config.DataConfiguration.ImageFlowLeftActions.Split(';').Contains("BackgroundUrl"))
                            {
                                // Set the background Uri
                                _context.EditorImage.Background = m.Url;
                            }
                            if (_context.Config.DataConfiguration.ImageFlowLeftActions.Split(';').Contains("Context"))
                            {
                                // Set the context
                                _context.EditorImage.Context = m.DataMessage;
                            }
                            _context.EditorImage.ReRender(true);
                            _context.EditorImage = _context.EditorImage;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // TODO HAndle and not generic Exceltion
            }
        }

        private void ImageFlowList_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var grid = sender as Grid;
                if (grid != null)
                {
                    var item = grid.DataContext as DataFlowItem;
                    if (item != null)
                    {
                        var m = item;

                        //if (_context.Config.DataConfiguration.AutoCartClearTypes.Split(';').Contains(m.DataMessage.DataType))
                        //{
                        //    // Need to clear the current active cart
                        //    _context.ActiveCart.Slides.Clear();
                        //}
                        //if (_context.Config.DataConfiguration.AutoCartAppendTypes.Split(';').Contains(m.DataMessage.DataType))
                        //{
                        //    // Need to append to current active cart
                        //    _context.CreateCartForDataFlowItem(m, true);
                        //}
                        //if (_context.Config.DataConfiguration.AutoCartGenerationTypes.Split(';').Contains(m.DataMessage.DataType))
                        //{
                        //    // Need to create a new cart
                        //    _context.CreateCartForDataFlowItem(m, false);
                        //}
                        if (_context.Config.DataConfiguration.AutoEditorTypes.Split(';').Contains(m.DataMessage.DataType))
                        {
                            // Need to update the editor's template
                            if (_context.Config.DataConfiguration.ImageFlowRightActions.Split(';').Contains("Template"))
                            {
                                // Load template to editor
                                // Find appropriate template
                                var conf = _context.Config.DataConfiguration.DataItemConfigurations.Cast<DataItemConfiguration>().FirstOrDefault(c => c.DataType == m.DataMessage.DataType);
                                // Load corresponding template
                                if (conf != null)
                                {
                                    var newTemplate = new ManagerImageReference(_context.Config, _context)
                                    {
                                        Template = conf.DefaultTemplate,
                                        Link = _context.Config.SlidesConfiguration.DefaultLink,
                                        CanRepeate = false
                                    };

                                    _context.EditorImage = newTemplate;
                                }
                            }
                            if (_context.Config.DataConfiguration.ImageFlowRightActions.Split(';').Contains("BackgroundUrl"))
                            {
                                // Set the background Uri
                                _context.EditorImage.Background = m.Url;
                            }
                            if (_context.Config.DataConfiguration.ImageFlowRightActions.Split(';').Contains("Context"))
                            {
                                // Set the context
                                _context.EditorImage.Context = m.DataMessage;
                            }
                            _context.EditorImage.ReRender(true);
                            _context.EditorImage = _context.EditorImage;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // TODO HAndle and not generic Exceltion
            }
        }

        private void ImageFlowList_Drop(object sender, DragEventArgs e)
        {

            Dispatcher.BeginInvoke(DispatcherPriority.Render,
          (SendOrPostCallback)delegate
          {
              _context = (ManagerContext)DataContext;
              if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
              {
                  string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                  if (droppedFilePaths != null)
                      foreach (var path in droppedFilePaths)
                      {
                          _context.IngestImage(path);
                      }
              }
          }, null);

        }

        private void ClearImageFlowFilter_Click(object sender, RoutedEventArgs e)
        {
            _context = (ManagerContext)DataContext;
            _context.ImageFlowFilterString = "";
        }

        #endregion ImageFLowManagement

        #region CartElementDragDrop

        private ListBoxItem _dragged;
        private void PreviewListBox_PreviewLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_dragged != null)
                return;

            UIElement element = PreviewCartListBox.InputHitTest(e.GetPosition(PreviewCartListBox)) as UIElement;

            while (element != null)
            {
                var item = element as ListBoxItem;
                if (item != null)
                {
                    _dragged = item;
                    break;
                }
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragged == null)
                return;
            if (e.LeftButton == MouseButtonState.Released)
            {
                _dragged = null;
                return;
            }
            // ReSharper disable once AssignNullToNotNullAttribute
            DataObject obj = new DataObject(DataFormats.Text, _dragged.ToString());
            DragDrop.DoDragDrop(_dragged, obj, DragDropEffects.All);

        }


        private void CartListElement_DragEnter(object sender, DragEventArgs e)
        {
            if (_dragged == null || e.Data.GetDataPresent(DataFormats.Text, true) == false)
                e.Effects = DragDropEffects.None;
            else
                e.Effects = DragDropEffects.All;
        }

        private void CartListElement_DragOver(object sender, DragEventArgs e)
        {
            CartListElement_DragEnter(sender, e);
        }

        private void CartListElement_Drop(object sender, DragEventArgs e)
        {
            // Add to cart
            var grid1 = sender as Grid;
            if (grid1 != null && _dragged != null)
            {
                var grid = grid1;
                var context = grid.DataContext as ManagerCart;
                if (context != null)
                {
                    // Remove from Preview Cart
                    var slide = (ManagerImageReference)(_dragged).DataContext;
                    if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    {
                        // CTRL is hold thus copy
                        // Clone and add
                        var cart = context;
                        cart.Slides.Add(slide.Clone());
                    }
                    else
                    {
                        // No CTRL thus move
                        // Remove
                        _context.PreviewCart.Slides.Remove(slide);
                        // Add to cart
                        var cart = context;
                        cart.Slides.Add(slide);
                    }

                }
            }
        }


        private void PreviewCartListBox_DragOver(object sender, DragEventArgs e)
        {

        }

        private void PreviewCartListBox_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (_dragged != null)
                {
                    var slide = (ManagerImageReference)(_dragged).DataContext;

                    ListBoxItem dropTo = null;
                    UIElement element = PreviewCartListBox.InputHitTest(e.GetPosition(PreviewCartListBox)) as UIElement;
                    while (element != null)
                    {
                        var to = element as ListBoxItem;
                        if (to != null)
                        {
                            dropTo = to;
                            break;
                        }
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (dropTo != null)
                    {
                        var dropToSlide = (ManagerImageReference)(dropTo).DataContext;
                        var dropIndex = _context.PreviewCart.Slides.IndexOf(dropToSlide);
                        var slideIndex = _context.PreviewCart.Slides.IndexOf(slide);
                        if (slideIndex < dropIndex)
                            dropIndex--;

                        _context.PreviewCart.Slides.Remove(slide);
                        _context.PreviewCart.Slides.Insert(dropIndex, slide);
                    }
                }
            }
            catch (Exception)
            {
                // TODO LOG
            }
        }

        #endregion CartElementDragDrop

        #region CartSelection

        private void CartListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var listBox = sender as ListBox;
                if (listBox != null)
                {
                    var cart = listBox.SelectedItem as ManagerCart;
                    if (cart != null)
                    {
                        _lastSelectedCart = cart;
                    }
                }
            }
            catch (Exception)
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
            var name = NewCartWindow.Show(this);
            if (!string.IsNullOrEmpty(name))
            {
                var c = new ManagerCart(name)
                {
                    CanBeDeleted = true,
                    ShowInCartList = true
                };

                _context.Carts.Add(c);
                _context.PreviewCart = c;
            }
        }

        #endregion CartSelection

        #region TemplateManagement

        private void TemplateListBox_Drop(object sender, DragEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
          (SendOrPostCallback)delegate
          {
              _context = (ManagerContext)DataContext;
              if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
              {
                  string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                  if (droppedFilePaths != null)
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

        private void TemplateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selected other Slide to be pushed !
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
            (SendOrPostCallback)delegate
            {
                _context = (ManagerContext)DataContext;
                if (e.AddedItems.Count > 0 && e.AddedItems[0] is ManagerImageReference)
                {
                    _lastSelectedEditorImageRef = (ManagerImageReference)e.AddedItems[0];
                    //_context.EditorImage = _lastSelectedEditorImageRef.Clone();
                }

            }, null);
        }

        private void TemplateList_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var grid = sender as Grid;
                if (grid != null)
                {
                    var template = grid.DataContext as ManagerImageReference;
                    if (template != null)
                    {
                        var newTemplate = template.Clone();
                        if (_context.EditorImage != null)
                        {
                            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                            {
                                // Keep existing context when CTRL is hold
                                var existingContext = _context.EditorImage.Context;
                                var existingBgImg = _context.EditorImage.Background;
                                newTemplate.Context = existingContext;
                                newTemplate.Background = existingBgImg;
                            }
                        }
                        _context.EditorImage = newTemplate;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO
                var a = ex;
            }
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

        private void resetTemplateCart_Click(object sender, RoutedEventArgs e)
        {
            _context.LoadCarts(true, true);
            _context.ResetAllPriorities();
        }

        #endregion TemplateManagement

        #region RunningCartManagement

        private void PreviewListBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Remove the last selected Slide from the active Cart
            if (e.Key == Key.Delete)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (SendOrPostCallback)delegate
                {
                    _context = (ManagerContext)DataContext;
                    if (_lastSelectedManagerImageRef != null)
                    {
                        if (_context.PreviewCart.Slides.Contains(_lastSelectedManagerImageRef) && _context.PreviewCart.Slides.Count > 1)
                        {
                            var idx = PreviewCartListBox.SelectedIndex;
                            _context.PreviewCart.Slides.Remove(_lastSelectedManagerImageRef);
                            _context.ReloadPreview();
                            PreviewCartListBox.SelectedIndex = Math.Max(0, Math.Min(idx, _context.PreviewCart.Slides.Count - 1));
                        }
                    }

                }, null);
            }
        }

        private void PreviewListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ManagerImageReference)
            {
                _lastSelectedManagerImageRef = (ManagerImageReference)e.AddedItems[0];
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
                    if (_lastSelectedManagerImageRef != null)
                    {
                        _lastAutomationChange = DateTime.Now;
                        _context.SwitchToSlide(_lastSelectedManagerImageRef);
                    }

                }, null);
            }
            if (e.ClickCount == 1 && e.RightButton == MouseButtonState.Pressed)
            {
                // Right mouse click, to edit
                _context = (ManagerContext)DataContext;
                if (_lastSelectedManagerImageRef != null)
                {
                    _context.EditorImage = _lastSelectedManagerImageRef;
                }

            }
        }
        #endregion RunningCartManagement

        #region LayoutEdition

        private void editButton_Unchecked(object sender, RoutedEventArgs e)
        {
            string company = "EBU";
            string application = "ContentManager";
            string windows = "MainWindow";
            RegistryHelper.SaveValue(company, application, windows, "VerticalColumn0", HorizontalSystemGrid.ColumnDefinitions[0].ActualWidth);
            RegistryHelper.SaveValue(company, application, windows, "VerticalColumn2", HorizontalSystemGrid.ColumnDefinitions[2].ActualWidth);
            RegistryHelper.SaveValue(company, application, windows, "HorizontalRow0", VerticalSystemGrid.RowDefinitions[0].ActualHeight);
            RegistryHelper.SaveValue(company, application, windows, "HorizontalRow2", VerticalSystemGrid.RowDefinitions[2].ActualHeight);
            RegistryHelper.SaveValue(company, application, windows, "HorizontalRow4", VerticalSystemGrid.RowDefinitions[4].ActualHeight);
        }

        private void RestoreLayout()
        {
            string company = "EBU";
            string application = "ContentManager";
            string windows = "MainWindow";
            try
            {
                if (RegistryHelper.GetDouble(company, application, windows, "VerticalColumn0") > 0.0)
                {
                    // We have settings, thus restore
                    HorizontalSystemGrid.ColumnDefinitions[0].Width =
                        new GridLength(RegistryHelper.GetDouble(company, application, windows, "VerticalColumn0"));
                    HorizontalSystemGrid.ColumnDefinitions[2].Width =
                        new GridLength(RegistryHelper.GetDouble(company, application, windows, "VerticalColumn2"));
                    VerticalSystemGrid.RowDefinitions[0].Height =
                        new GridLength(RegistryHelper.GetDouble(company, application, windows, "HorizontalRow0"));
                    VerticalSystemGrid.RowDefinitions[2].Height =
                        new GridLength(RegistryHelper.GetDouble(company, application, windows, "HorizontalRow2"));
                    VerticalSystemGrid.RowDefinitions[4].Height =
                        new GridLength(RegistryHelper.GetDouble(company, application, windows, "HorizontalRow4"));
                }
            }
            catch (Exception)
            {
                // TODO Log
            }
        }

        #endregion LayoutEdition











    }
}
