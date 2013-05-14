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
using SlideGeneratorLib;
using System.Threading;

namespace ContentManager.GUI.Frames
{
    /// <summary>
    /// Interaction logic for HotSlides.xaml
    /// </summary>
    public partial class HotSlides : UserControl
    {
        public HotSlides()
        {
            InitializeComponent();

           
        }

        internal void loadSlides()
        {
            List<String> availableSlides = UIMain.core.slidegen.getAvailableSlides();

            for (int i = 0; i < availableSlides.Count; i++)
            {
          /*      Thread t = new Thread(new ThreadStart(delegate()
                {
                    SlideResult res = UIMain.core.slidegen.loadXMLSlide(availableSlides[i]);
                    stack.Children.Add(res.image);
                }));
                
                //stack.Children.Add();*/
                String slideName= availableSlides[i];
                Button btn = new Button();
                btn.Content = slideName;
                btn.Tag = slideName;
                btn.MinWidth = 100;
                btn.Height = 30;
                btn.Click += new RoutedEventHandler(btn_Click);
                btn.MouseRightButtonDown += new MouseButtonEventHandler(btn_MouseRightButtonDown);
                btn.Padding = new Thickness(10,3,10,3);
                //SlideResult r = UIMain.core.slidegen.loadXMLSlide(availableSlides[i]);
                stack.Children.Add(btn);
            }
        }

        void btn_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button btn = (Button)sender;

            
            UIMain.Instance.displayPreviewSlide(btn.Tag.ToString());
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button) sender;

            UIMain.core.broadcast(btn.Tag.ToString());
        }

        


    }
}
