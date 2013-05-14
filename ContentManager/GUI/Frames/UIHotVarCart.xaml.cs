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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContentManager.DataStructure;

namespace ContentManager.GUI.Frames
{
    /// <summary>
    /// Interaction logic for UIHotVarCart.xaml
    /// </summary>
    public partial class UIHotVarCart : UserControl
    {
        public UIHotVarCart()
        {
            InitializeComponent();
        }

        public void loadCart(){

            LinkedList<VarCart> sc = UIMain.Instance.uicartedit.varCart;
            this.stackCarts.Children.Clear();
            Console.WriteLine("yoyo");
            
            foreach (VarCart s in sc)
            {
                Button b = new Button();
                b.Content = s.name;
                b.Tag = s;
                b.Foreground = Brushes.White;
                b.SetResourceReference(BackgroundProperty, "BlackBtn");
                b.Click += new RoutedEventHandler(VarCart_Click);
                b.MouseRightButtonDown += new MouseButtonEventHandler(b_MouseRightButtonDown);
                this.stackCarts.Children.Add(b);
            }
        }

        public void VarCart_Click(object Sender, RoutedEventArgs Event)
        {
            Button b = (Button)Sender;

            VarCart s = (VarCart)b.Tag;
            foreach (KeyValuePair<String, String> vp in s.variables)
            {
               updateList(s.variables);
            }

        }
        private void updateList(Dictionary<string, string> variables)
        {
            foreach (KeyValuePair<String, String> p in variables)
            {
                if (UIMain.core.slidegen.cstlist.ContainsKey(p.Key.ToUpper()))
                    UIMain.core.slidegen.cstlist[p.Key.ToUpper()] = p.Value;
                else
                    UIMain.core.slidegen.cstlist.Add(p.Key.ToUpper(), p.Value);
            }
        }

        public void b_MouseRightButtonDown(object Sender, MouseButtonEventArgs evt)
        {

            Button b = (Button)Sender;

            VarCart s = (VarCart)b.Tag;
            String msg = "";
            foreach(KeyValuePair<String,String> vp in  s.variables){
                msg += vp.Key + ": " + vp.Value+"\n";

            }


            MessageBox.Show(msg);

        }

        internal void refreshVarCarts()
        {
            
            loadCart();
        }
    }
}
