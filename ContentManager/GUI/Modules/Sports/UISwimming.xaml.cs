using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using log4net;
using System.Windows.Threading;
using CsvHelper.Configuration;
using ContentManager.GUI.Modules.Sports.SwisstimingData;
using System.IO;
using CsvHelper;

namespace ContentManager.GUI.Modules.SwissTiming
{
    /// <summary>
    /// Interaction logic for Swimming.xaml
    /// </summary>
    public partial class UISwimming : Window
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(UISwimming));

        CsvConfiguration csvConfig = new CsvConfiguration();
        public UISwimming()
        {
            InitializeComponent();
            UIMain.core.amqpEngine.onTrace += new AMQPEngine.Trace(amqpEngine_onTrace);
            UIMain.core.amqpEngine.onData += new AMQPEngine.Data(amqpEngine_onData);

            csvConfig.Delimiter = ";";
            csvConfig.IsStrictMode = true;
        }

        public Dictionary<String, LinkedList<Dictionary<String, String>>> swimmingDatabase = new Dictionary<string, LinkedList<Dictionary<string, string>>>();

        /**
         * When receiving CSV data through the AMQP listener, it is stored in an in memory buffer (swimmingDatabase).
         * The first key represents the path to the data including the filename without .csv
         * 
         * 
         */
        void amqpEngine_onData(Dictionary<string, string> message)
        {
            DateTime startTiming = new DateTime();

            String path = message["path"].Split('.')[0];
            //String dataLevel = message["path"].Split('/').Last().Split('.')[0];

            string csvData = message["data"];

            var csv = new CsvReader(new StringReader(csvData), csvConfig);

            LinkedList<Dictionary<String, String>> records = new LinkedList<Dictionary<String, String>>();
            int recordNumber = 0;
            while (csv.Read())
            {
                var record = new Dictionary<string, string>();
                for (int i = 0; i < csv.FieldHeaders.Count(); i++)
                {
                    record.Add(csv.FieldHeaders[i], csv.GetField(i));
                    UIMain.core.slidegen.setVar(path+"@"+csv.FieldHeaders[i]+"#"+recordNumber.ToString(), csv.GetField(i));
                }
                records.AddLast(record);
                recordNumber++;
            }

            if (!swimmingDatabase.ContainsKey(path))
            {
                swimmingDatabase.Add(path, records);
            }
            else
            {
                //Update : Should trigger a Event
                swimmingDatabase[path] = records;
            }

            UIMain.core.slidegen.setVar("AMQP_LAST_PATH", path);

            log.Debug("Update propagated in " + (new DateTime() - startTiming).TotalMilliseconds + "ms");
        }


        void amqpEngine_onTrace(Dictionary<string, string> message)
        {
            if (message["cmd"].Equals("mkdir") && !message["data"].Contains("."))
            {
                log.Debug("new path : " + message["data"] + " ");

                var splittedPath = message["data"].Split('/');
                if (splittedPath.First().Equals("")) splittedPath = splittedPath.Skip(1).ToArray();

                this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    List<TreeViewItem> itemsToHighlight = new List<TreeViewItem>();
                    var currentItemList = tree.Items;
                    String currentPath = "/";
                    for (int i = 0; i < splittedPath.Count(); i++)
                    {
                        String folder = splittedPath[i];

                        TreeViewItem newItem = new TreeViewItem();
                        newItem.Header = folder;
                        newItem.Tag = currentPath;
                        newItem.ExpandSubtree();
                        newItem.Selected += new RoutedEventHandler(newItem_Selected);

                        TreeViewItem existingItem = null;
                        foreach (TreeViewItem item in currentItemList)
                        {
                            if (item.Header.Equals(newItem.Header))
                            {
                                existingItem = item;
                                break;
                            }
                        }

                        if (existingItem == null)
                        {
                            currentItemList.Add(newItem);
                            currentItemList = newItem.Items;
                            itemsToHighlight.Add(newItem);
                        }
                        else
                        {
                            currentItemList = existingItem.Items;
                            itemsToHighlight.Add(existingItem);
                        }
                    }
                    setHighlight(itemsToHighlight);
                }));
            }
        }

        void newItem_Selected(object sender, RoutedEventArgs e)
        {
            Title = ((TreeViewItem)sender).Tag.ToString();
        }

        List<TreeViewItem> highlightedItems = new List<TreeViewItem>();
        void setHighlight(List<TreeViewItem> itemsToHighlight)
        {
            foreach (var item in highlightedItems) item.Foreground = Brushes.Black;
            highlightedItems.Clear();
            foreach (var item in itemsToHighlight)
            {
                item.Foreground = Brushes.Lime;
                highlightedItems.Add(item);
            }
        }

    }
}
