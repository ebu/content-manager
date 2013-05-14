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
using SlideGeneratorLib.Parser;

namespace ContentManager.GUI.Frames
{
    /// <summary>
    /// Interaction logic for VarEditFrame.xaml
    /// </summary>
    public partial class VarEditFrame : UserControl
    {
        public VarEditFrame()
        {
            InitializeComponent();

        }

        private void availableSlidesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            String slideName = availableSlidesListBox.SelectedItem.ToString();
            List<String> l = UIMain.core.slidegen.getSlideVariables(slideName);
            this.stack.Children.Clear();
            for (int i = 0; i < l.Count; i++)
            {
                String key = l[i].ToUpper();
                String value = "";
                if(UIMain.core.slidegen.cstlist.ContainsKey(key))
                    value = UIMain.core.slidegen.cstlist[key];
                
                TextBox t = newVarRow(key,value);
                //Freeze PROTECTED VARS on display
                if (VarParser.getProtectedVariables().Contains(key))
                {
                    t.IsEnabled = false;
                    t.Text = VarParser.parseText("@@@" + key + "@@@", null, null);
                }
            }
               

            /*availableVariablesListBox.Items.Clear();
            String slideName = availableSlidesListBox.SelectedItem.ToString();
            List<String> l = UIMain.core.slidegen.getSlideVariables(slideName);
            for (int i = 0; i < l.Count; i++)
                availableVariablesListBox.Items.Add(l.ElementAt(i));

            availableVariablesListBox.IsEnabled = (availableVariablesListBox.Items.Count != 0);*/
        }

        private TextBox newVarRow(String varname, String varvalue)
        {
            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions[0].Width = new GridLength(100);
            TextBox b = new TextBox();
            g.Children.Add(b);
            Grid.SetColumn(b, 1);
            TextBlock t = new TextBlock();
            g.Children.Add(t);
            Grid.SetColumn(t, 0);

            t.Foreground = Brushes.White;
            t.Text = varname;
            b.Tag = varname;
            b.TextAlignment = TextAlignment.Right;
            b.Text = varvalue;
            b.TextChanged += new TextChangedEventHandler(b_TextChanged);
            b.KeyDown += new KeyEventHandler(b_KeyDown);

            g.InvalidateVisual();
            this.stack.Children.Add(g);

            return b;
        }

        void b_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {

                UIMain.Instance.displayPreviewSlide(this.availableSlidesListBox.SelectedItem.ToString());
            }
            if (e.Key == Key.F8)
            {
                TextBox s = sender as TextBox;
                UIRssWizard uirss = new UIRssWizard(s, s.Text);

            }
        }

        void b_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox b = sender as TextBox;
            Dictionary<string, string> cst = UIMain.core.slidegen.cstlist;
            if (cst.ContainsKey(b.Tag.ToString()))
                cst[b.Tag.ToString()] = b.Text;
            else
                cst.Add(b.Tag.ToString(), b.Text);
        }
    }
}
