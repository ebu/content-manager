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
using XMLConfig.CMS;
using System.IO;
using SlideGeneratorLib;
using System.Windows.Threading;

namespace ContentManager.GUI
{
    /// <summary>
    /// Interaction logic for UIVideoCapture.xaml
    /// 
    /// Supported format of capture: http://msdn.microsoft.com/en-us/library/cc189080(v=VS.95).aspx
    /// </summary>
    public partial class UIVideoCapture : Window
    {
        private UIMain uIMain;
        private DispatcherTimer t;


        void t_Tick(object sender, EventArgs e)
        {
            int interval=0;
            if (Int32.TryParse(inputRate.Text.ToString(), out interval))
            {
                t.Interval = TimeSpan.FromSeconds(interval);
            }

            Console.WriteLine("TICK");
            Button_Capture(sender, null);            
        }

        
        public UIVideoCapture(UIMain uIMain)
        {
            this.uIMain = uIMain;
            InitializeComponent();
            Console.WriteLine("yep");

            t = new DispatcherTimer();
            t.Tick += new EventHandler(t_Tick);
            t.Interval = TimeSpan.FromSeconds(1);
            t.Start();

            this.Closing += new System.ComponentModel.CancelEventHandler(UIVideoCapture_Closing);
        }

        void UIVideoCapture_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            t.Stop();
            videoDisplay.Stop();
            videoDisplay.Close();
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            videoDisplay.Source = new Uri(inputURI.Text);
            videoDisplay.Play();
        }


        private void Button_Capture(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(CMSConfig.video_tmpfolder))
                Directory.CreateDirectory(CMSConfig.video_tmpfolder);

            String filename = CMSConfig.video_tmpfolder+"/" + DateTime.Now.ToFileTimeUtc().ToString() + "-capture.jpg";

            SlideGenerator.saveToJpg(videoDisplay, filename, "", false, 100);
            
            Dictionary<string, string> cst = UIMain.core.slidegen.cstlist;
            if (cst.ContainsKey(inputVar.Text.ToString()))
                cst[inputVar.Text.ToString()] = filename;
            else
                cst.Add(inputVar.Text.ToString(), filename);
        }

        private void videoDisplay_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            t.Stop();
            videoDisplay.Stop();
            videoDisplay.Close();
            videoDisplay.Source = null;  
            MessageBox.Show("Media Failed");
        }


        
    }
}
