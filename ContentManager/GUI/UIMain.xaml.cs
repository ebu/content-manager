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
using ContentServiceLibrary;
using System.ServiceModel;
using System.Windows.Threading;
using System.IO;
using System.Deployment.Application;
using System.Reflection;
using XMLConfig.CMS;
using System.Net;

namespace ContentManager.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UIMain : Window
    {

        UIDebug uidebug;
        public static ContentManagerCore core;
        public static UIMain Instance;
        public static int timerValue=0;
        public DispatcherTimer timerRemain;
        public UICartEdit uicartedit;
        public UIMain()
        {


            uidebug = new UIDebug();
            //CMSConfig.load
            loadconfig();
            
            //Init core
            core = ContentManagerCore.getInstance();

            

            InitializeComponent();
            Instance = this;
            this.Topmost = false;
            if (ContentManagerCore.debug)
                UIMain.errorAdd("DEBUG mode activated");
            else
                UIMain.errorAdd("DEBUG mode not activated");

            printVersion();

            loadPreviewAvailableSlides();

            uicartedit = new UICartEdit();
            //uicartedit.Show();

            core.engine.onBroadcast += new BroadcastEngine.OutputEvent(engine_onBroadcast);
            
            timerRemain = new DispatcherTimer();
            timerRemain.Interval = TimeSpan.FromMilliseconds(100);
            timerRemain.Tick += new EventHandler(timerRemain_Tick);
            timerRemain.Start();

            this.hotslides.loadSlides();
            this.onaircart.setEvents();
            this.onaircart.refreshSlideCartList();
            this.hotvars.refreshVarCarts();

            if (CMSConfig.ftp.Count == 0)
            {
                noFtpLabel.Visibility = System.Windows.Visibility.Visible;
                noFtpLabel2.Visibility = System.Windows.Visibility.Visible;
                panel2.Background = Brushes.Maroon;
            }


            if(CMSConfig.stationimage !="")
                stationPic.Source = new BitmapImage(new Uri(CMSConfig.stationimage));
        }


        private void printVersion()
        {
            String version = getVersion();
            if (ApplicationDeployment.IsNetworkDeployed)
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            this.labelAbout.Content = "EBU Content Manager - www.ebulabs.org - version: " + version;
        }

        private void loadconfig()
        {
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains("DEBUG"))
                System.Diagnostics.Debugger.Launch();
       
            try
            {
                String datafolder = System.Configuration.ConfigurationManager.AppSettings["DataFolder"];
                    
                if (!Directory.Exists(datafolder) && !File.Exists(datafolder + "\\config.xml") && ApplicationDeployment.IsNetworkDeployed)
                    datafolder = ApplicationDeployment.CurrentDeployment.DataDirectory + "\\" + datafolder;
                String cf = datafolder + "config.xml";
                if (!File.Exists(cf))
                {
                    MessageBox.Show("You can download a demo show at www.ebulabs.org.", "First run");
                    
                    UIConfig uicfg = new UIConfig(false, cf);
                    uicfg.ShowDialog();
                    
                }
                
                
                CMSConfig.load(cf);
                
            }
            catch (FileNotFoundException e)
            {
                MessageBox.Show("File " + System.Configuration.ConfigurationManager.AppSettings["DataFolder"] + "config.xml not found");
                Environment.Exit(1);
            }
            



        }
        void timerRemain_Tick(object sender, EventArgs e)
        {
            if (core.engine.isAutoBroadcastEnabled())
            {
                this.timerProgressBar.Visibility = Visibility.Visible;
                double delta = timerValue * 100 - core.engine.getInterval().TotalMilliseconds;
                if (delta <= 0)
                {
                    String str = TimeSpan.FromMilliseconds(delta).ToString();
                    if (str.Length >= 12)
                        str = str.Substring(0, 11);
                    else
                        str = str + ".0";
                    this.timerLabel.Text = str;
                    timerValue += 1;

                    timerProgressBar.Maximum = core.engine.getInterval().TotalMilliseconds;
                    timerProgressBar.Value = timerValue * 100;
                }
                else
                    this.timerLabel.Text = "";
            }
            else
            {
                this.timerLabel.Text = "";
                this.timerProgressBar.Visibility = Visibility.Hidden;
            }
        }

        private void loadPreviewAvailableSlides()
        {
            loadPreviewAvailableSlides(this.watchframe.availableSlidesListBox);
            loadPreviewAvailableSlides(this.varedit.availableSlidesListBox);
            loadPreviewAvailableSlides(availableSlidesListBox);


            List<String> templates = core.slidegen.getAvailableTemplates();
            availableTemplatesListBox.Items.Clear();
            foreach (String t in templates)
                availableTemplatesListBox.Items.Add(t);

            templatesMenu.Items.Clear();
            foreach (String t in templates){
                MenuItem m = new MenuItem();
                m.Header = t;
                if (t == core.slidegen.getCurrentTemplate())
                {
                    m.IsChecked = true;
                    currentTemplateLabel.Text = t;
                }
                m.Foreground = Brushes.Black;
                m.Click += new RoutedEventHandler(template_Click);
                templatesMenu.Items.Add(m);
            }
        }

        void template_Click(object sender, RoutedEventArgs e)
        {
            String tname = ((MenuItem)sender).Header.ToString();
            if (tname != "")
                core.engine.slidegen.setCurrentTemplate(tname);
            loadPreviewAvailableSlides();
        }

        private void loadPreviewAvailableSlides(ComboBox box)
        {
            List<String> slidenames = core.slidegen.getAvailableSlides();
            box.Items.Clear();

            foreach(String slidename in slidenames)
                box.Items.Add(slidename);

        }

        void engine_onBroadcast(string filename, string link)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                //errorlist.Items.Add("onair : " + autobroadcast.slideOnair);
                // Canvas s = slidegen.loadXMLSlide(autobroadcast.slideOnair);
                Image s = new Image();
                
                s.Source = loadImage(filename);
                RenderOptions.SetBitmapScalingMode(s, BitmapScalingMode.HighQuality);
                if (s != null)
                    panel2.Children.Add(s);
                
                if (panel2.Children.Count > 1)
                    panel2.Children.RemoveAt(0);
            }));
        }

        private BitmapImage loadImage(String filename)
        {
            
            FileStream f = new FileStream(SlideGenerator.tmpfolder + "" + filename, FileMode.Open);
            FileStream f2 = new FileStream(SlideGenerator.tmpfolder + "tmp" + filename, FileMode.CreateNew);
            
            f.CopyTo(f2);
            f.Close();
            f2.Close();

            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(SlideGenerator.tmpfolder+"tmp"+filename);
            myBitmapImage.EndInit();
            return myBitmapImage;
        }

        /*  void autobroadcast_onBroadcast(object sender, EventArgs e)
          {
                     this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                 {
                     errorlist.Items.Add("onair : " + autobroadcast.slideOnair);
                     // Canvas s = slidegen.loadXMLSlide(autobroadcast.slideOnair);
                      if (s != null)
                          panel2.Children.Add(s);

                      if (panel2.Children.Count > 1)
                          panel2.Children.RemoveAt(0);
                 }));
          }
          */
        public static void errorAdd(String msg, String module = "undefined")
        {
            if (Instance != null)
            {
                errorListAdd(msg, module);
            }
        }
        private static void errorListAdd(String msg, String module = "undefined")
        {
            if (Instance != null)
            {
                Instance.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    Instance.uidebug.errorlist.Items.Add("[" + module + "] " + DateTime.Now.ToLongTimeString() + ": " + msg);
                }));
            }
        }
        public static void fatalError(String msg)
        {
            MessageBox.Show(msg, "FATAL ERROR");
            Environment.Exit(1);            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            
            String slideName = availableSlidesListBox.Items.GetItemAt(availableSlidesListBox.SelectedIndex).ToString(); ;
            displayPreviewSlide(slideName);
            
        }

        public void displayPreviewSlide(String slideName)
        {
            SlideResult res = core.slidegen.loadXMLSlide(slideName);
            if (res != null)
            {
                Canvas s = res.image;
                
                if (s != null)
                    panel.Children.Add(s);

                if (panel.Children.Count > 1)
                    panel.Children.RemoveAt(0);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            if (Directory.Exists(CMSConfig.video_tmpfolder))
            {
                var v = Directory.EnumerateFiles(CMSConfig.video_tmpfolder);
                foreach (var f in v) 
                    File.Delete(f);
                Directory.Delete(CMSConfig.video_tmpfolder);
            }

            Environment.Exit(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String str = "";
            str += "Variables :\n";
            for (int i = 0; i < core.slidegen.cstlist.Count; i++)
            {
                str += core.slidegen.cstlist.ElementAt(i).Key + "=" +core.slidegen.cstlist.ElementAt(i).Value;
                str += "\n";
            }
            MessageBox.Show(str);
        }
        
        private void BClick_debug(object sender, RoutedEventArgs e)
        {
            //errorlist.Visibility = (errorlist.Visibility == System.Windows.Visibility.Hidden) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            this.uidebug.Show();
        }

    
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            String str = "";
            str += "Slides :\n";
            List<String> slidenames = core.slidegen.getAvailableSlides();
            
            for (int i = 0; i < slidenames.Count; i++)
            {
                str += slidenames[i];
                str += "\n";
            }
            MessageBox.Show(str);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(core.datafolder);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Log.getLogFile());
        }


        private String getVersion()
        {
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            return ver.ToString();
        }

        private void availableTemplatesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String tname = availableTemplatesListBox.Items.GetItemAt(availableTemplatesListBox.SelectedIndex).ToString();
            if (tname != "")
                core.engine.slidegen.setCurrentTemplate(tname);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            String datafolder = System.Configuration.ConfigurationManager.AppSettings["DataFolder"];

            if (!Directory.Exists(datafolder) && !File.Exists(datafolder + "\\config.xml") && ApplicationDeployment.IsNetworkDeployed)
                datafolder = ApplicationDeployment.CurrentDeployment.DataDirectory + "\\" + datafolder;
        
            UIConfig cfg = new UIConfig(true, datafolder+"\\config.xml");
            cfg.Show();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            core.engine.stopAutoBroadcast();
            pmLabel.Text = "Manual";
            pmLabel.Style = this.Resources["BannerInlayTextOA"] as Style;
        }

        private void pmAutoBtn_Click(object sender, RoutedEventArgs e)
        {
            core.engine.startAutoBroadcast();
            pmLabel.Text = "Automatic";
            pmLabel.Style = this.Resources["BannerInlayTextBlue"] as Style;
            
        }
        //Refresh
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            this.uicartedit.reload();
            hotvars.refreshVarCarts();
            onaircart.refreshSlideCartList();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            String version = getVersion();
            if (ApplicationDeployment.IsNetworkDeployed) { version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString(); }
            MessageBox.Show("EBU Technology & Development\nwww.ebulabs.org\n\nversion: " + version +"\n\nMathias Coinchon (coinchon@ebu.ch)\nMichael Barroco (barroco@ebu.ch)");
        }


        //Change Slide Folder
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderPicker = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult r = folderPicker.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.OK)
            {

               
                    try
                    {
                        core.slidegen.setSlideDir(folderPicker.SelectedPath);
                    }
                    catch
                    {
                        
                    }
                    core.slidegen.getAvailableSlides();
                    
                    core.checkAfterChangeDir();

                    this.hotslides.loadSlides();
                    this.onaircart.setEvents();
                    this.onaircart.refreshSlideCartList();
                    this.hotvars.refreshVarCarts();

            }
               
            
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            UIVideoCapture videoCapture = new UIVideoCapture(this);
            videoCapture.Show();
        }
    }
}
