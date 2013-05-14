/*
Copyright (C) 2010-2012 European Broadcasting Union
http://www.ebulabs.org
*/
/*
This file is part of ebu-content-manager.
https://code.google.com/p/ebu-content-manager/

EBU-content-manager is free software: you can redistribute it and/or modify
it under the terms of the GNU LESSER GENERAL PUBLIC LICENSE as
published by the Free Software Foundation, version 3.
EBU-content-manager is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU LESSER GENERAL PUBLIC LICENSE for more details.

You should have received a copy of the GNU LESSER GENERAL PUBLIC LICENSE
along with EBU-content-manager.  If not, see <http://www.gnu.org/licenses/>.
*/
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
using System.Xml;
using SlideGeneratorLib.Parser;

namespace ContentManager.GUI
{
    /// <summary>
    /// Interaction logic for UIRssWizard.xaml
    /// </summary>
    public partial class UIRssWizard : Window
    {
        public UIRssWizard()
        {
            InitializeComponent();
        }

        public UIRssWizard(TextBox uicaller, String url ="")
        {
            InitializeComponent();
            this.uicaller = uicaller;
            try
            {
                if (url.StartsWith("rss@"))
                {
                    url = url.Substring(4);
                    Console.WriteLine(url);
                    String[] surl = url.Split(("@").ToArray());
                    Console.WriteLine(surl[0]);
                    url = surl[0];
                    rssUrl.Text = url;
                    Button_Click(null, null);

                    String[] sep = { ">", "!" };
                    List<String> path = surl[1].Split(sep, StringSplitOptions.None).ToList();
                    String s = "";
                    TreeViewItem current = rssTree.Items[0] as TreeViewItem;
                    current.IsExpanded = true;
                    current = current.Items[0] as TreeViewItem;
                    current.IsExpanded = true;

                    expandPath(current, path);


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            ShowDialog();
        }
        private void expandPath(TreeViewItem item, List<String> path){
            if (path.Count == 0) return;
            for(int i=0; i< item.Items.Count; i++){
                TreeViewItem t = item.Items[i] as TreeViewItem;
                if (t.Header.ToString().Equals(path.First()))
                {
                    t.IsExpanded = true;
                    path.RemoveAt(0);
                    if (path.Count == 0)
                        t.IsSelected = true;
                    else
                        expandPath(t, path);
                    break;
                }
            }

        }
        XmlDocument xmldoc = null;
        private TextBox uicaller;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String url = this.rssUrl.Text;
            try
            {
                xmldoc = new XmlDocument();
                xmldoc.Load(url);
                rssTree.Items.Clear();
                rssTree.Items.Add(foldChild(xmldoc.ChildNodes, xmldoc.FirstChild, ""));
                rssTree.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(rssTree_SelectedItemChanged);
            }
            catch (Exception er)
            {
                MessageBox.Show("Error when trying to retrieve rss stream\n"+er.Message);
            }

        }

        void rssTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem i = rssTree.SelectedItem as TreeViewItem;
            if (i == null)
                rssResult.Text = "";
            else if (i.Tag != null)
            {
                rssResult.Text = "rss@" + rssUrl.Text + "@" + i.Tag.ToString();
                rssPreview.Text = VarParser.parseRSS(rssResult.Text, xmldoc);
            }
            else
                rssResult.Text = "not supported";
        }

        TreeViewItem foldChild(XmlNodeList nodes, XmlNode parentNode, String path = "", String newname = "")
        {
            TreeViewItem t = new TreeViewItem();
            t.Header = (newname != "") ? newname : parentNode.Name;

            if (path != "")
                path = path + ">" + ((newname != "") ? newname : parentNode.Name);
            else
                path = ((newname != "") ? newname : parentNode.Name);

            if (path.StartsWith("xml>"))
                path = path.Substring(4);


            if (parentNode.Attributes != null)
            {
                for (int j = 0; j < parentNode.Attributes.Count; j++)
                {
                    TreeViewItem a = new TreeViewItem();
                    a.Header = parentNode.Attributes[j].Name;
                    a.Tag = path + "!" + parentNode.Attributes[j].Name;
                    a.Foreground = Brushes.BlueViolet;
                    t.Items.Add(a);

                }

            }



            Dictionary<String, int> counter = new Dictionary<string, int>();

            Dictionary<String, int> nb = firstPassCounter(nodes);

            for (int i = 0; i < nodes.Count; i++)
            {
                //    rssList.Items.Add(nodes[i].Name);
                String nname = "";
                if (counterGet(nb, nodes[i].Name) != 1)
                {
                    counterAdd(counter, nodes[i].Name);
                    nname = nodes[i].Name + "(" + counterGet(counter, nodes[i].Name) + ")";
                }


                if (nodes[i].HasChildNodes && nodes[i].NodeType == XmlNodeType.Element)
                {
                    TreeViewItem item = foldChild(nodes[i].ChildNodes, nodes[i], path, nname);

                    t.Items.Add(item);

                }
                else if (nodes[i].NodeType == XmlNodeType.Element)
                {
                    TreeViewItem t2 = new TreeViewItem();
                    t2.Header = (nname != "") ? nname : nodes[i].Name;
                    t2.Foreground = Brushes.Red;
                    for (int j = 0; j < nodes[i].Attributes.Count; j++)
                    {
                        TreeViewItem a = new TreeViewItem();
                        a.Header = nodes[i].Attributes[j].Name;
                        a.Foreground = Brushes.BlueViolet;
                        a.Tag = path + ">" + t2.Header + "!" + nodes[i].Attributes[j].Name;

                        t2.Items.Add(a);

                    }
                    t.Items.Add(t2);
                }
                else if (nodes.Count == 1)
                {
                    t.Foreground = Brushes.Green;
                    t.Tag = path;
                }
                else
                    t.IsExpanded = true;
            }






            return t;
        }


        private Dictionary<String, int> firstPassCounter(XmlNodeList nodes)
        {
            Dictionary<String, int> counter = new Dictionary<string, int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                counterAdd(counter, nodes[i].Name);
            }
            return counter;
        }
        private void counterAdd(Dictionary<String, int> counter, String name)
        {
            if (!counter.ContainsKey(name))
                counter.Add(name, 1);
            else
                counter[name] = counter[name] + 1;
        }
        private int counterGet(Dictionary<String, int> counter, String name)
        {
            if (!counter.ContainsKey(name))
                return 1;
            else
                return counter[name];
        }

        private void rssTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem i = (TreeViewItem)rssTree.SelectedItem;

            if (i.Tag != null && uicaller != null)
            {
                uicaller.Text = "rss@" + rssUrl.Text + "@" + i.Tag.ToString();
                Close();
            }


        }


    }
}
