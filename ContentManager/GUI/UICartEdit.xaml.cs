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
using ContentManager.DataStructure;

namespace ContentManager.GUI
{
    /// <summary>
    /// Interaction logic for UICartEdit.xaml
    /// </summary>
    public partial class UICartEdit : Window
    {

        public LinkedList<SlideCart> slideCart = new LinkedList<SlideCart>();
        public LinkedList<VarCart> varCart = new LinkedList<VarCart>();

        public UICartEdit()
        {
            InitializeComponent();

            this.Topmost = true;

            reload();
        }

        public void reload()
        {

            loadSlideCarts();
            refresh();
        }

        private void refresh()
        {
            this.listSlideCartSelection.Items.Clear();
            for (int i = 0; i < slideCart.Count; i++)
            {
                this.listSlideCartSelection.Items.Add(slideCart.ElementAt(i));
            }

        
        }

        private void loadSlideCarts()
        {
            String path = SlideGeneratorLib.SlideGenerator.datafolder + "slidecarts.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(path);



            this.slideCart.Clear();
            this.varCart.Clear();
            XmlNodeList carts = doc.GetElementsByTagName("slidecart");
            for (int i = 0; i < carts.Count; i++)
            {
                String name = carts[i].Attributes["name"].Value;
                SlideCart slidec = new SlideCart(name);
                

                XmlNodeList node = carts[i].ChildNodes;
                for (int j = 0; j < node.Count; j++)
                {
                    if (node[j].Name.ToString().Equals("slide"))
                    {
                        slidec.slides.Add(node[j].Attributes["name"].Value);
                    }
                    else if (node[j].Name.ToString().Equals("variable"))
                    {
                        slidec.variables.Add(node[j].Attributes["name"].Value, node[j].Attributes["value"].Value);
                    }
                }
                this.slideCart.AddLast(slidec);
            }

            XmlNodeList varcarts = doc.GetElementsByTagName("varcart");
            for (int i = 0; i < varcarts.Count; i++)
            {
                String name = varcarts[i].Attributes["name"].Value;
                VarCart varcart = new VarCart(name);


                XmlNodeList node = varcarts[i].ChildNodes;
                for (int j = 0; j < node.Count; j++)
                {

                    if (node[j].Name.ToString().Equals("variable"))
                    {
                        varcart.variables.Add(node[j].Attributes["name"].Value, node[j].Attributes["value"].Value);
                    }
                }
                this.varCart.AddLast(varcart);
            }
        }

        private void VariablesFrame_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            listSlideCartSelection.Items.Add(new SlideCart(txtNewCart.Text));
        }

        private void listSlideCartSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SlideCart sc = ((SlideCart)listSlideCartSelection.SelectedItem);
            this.cartPreview.IsEnabled = true;
            this.cartPreview.refresh(sc);
            this.cartVariables.IsEnabled = true;
            this.cartVariables.refresh(sc);
            this.btnSave.IsEnabled = true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
