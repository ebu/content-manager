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
using System.Windows;
using XMLConfig.CMS;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;

namespace ContentManager.GUI
{
    /// <summary>
    /// Interaction logic for UIConfig.xaml
    /// </summary>
    public partial class UIConfig : Window
    {
        private bool p;
        private string cf;


        public UIConfig(bool p, string cf)
        {
            // TODO: Complete member initialization
            this.p = p;
            this.cf = cf;
            
            InitializeComponent();
            if (p)
                loadConfig();
            else
                loadDefaultConfig();

            exportbtn.IsEnabled = true;
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.zStompTopicsList.Items.Add(this.nStompTopic.Text);
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int ind = this.zStompTopicsList.SelectedIndex;
            if (ind != -1)
                this.zStompTopicsList.Items.RemoveAt(ind);
        }

        private void loadConfig()
        {
            this.zBroadcastDelay.Text = CMSConfig.broadcastdelay.ToString();
            this.zSlideCartPrefix.Text = CMSConfig.slidecartprefix;
            this.zTemplateDir.Text = CMSConfig.dirtemplate;
            this.zSlideDir.Text = CMSConfig.dirslide;
            this.zWatchDir.Text = CMSConfig.dirwatch;
            this.zHttpPort.Text = CMSConfig.httpport.ToString();
            this.zHttpUrlSeparator.Text = CMSConfig.httpurlseparator;
            this.zRadioVisLink.Text = CMSConfig.radiovislink;
            this.zRadioVisText.Text = CMSConfig.radiovistxt;
            this.zStompHttpUrl.Text = CMSConfig.stomphttpurl;
            this.zStompLogin.Text = CMSConfig.stomplogin;
            this.zStompPasscode.Text = CMSConfig.stomppasscode;
            this.zStompServer.Text = CMSConfig.stompserver;
            this.zStompPort.Text = CMSConfig.stompport.ToString();
            foreach (String s in CMSConfig.stomptopic)
                this.zStompTopicsList.Items.Add(s);

           this.zTypeList.Text =CMSConfig.inputtype.ToUpper();
            foreach (FtpAccount f in CMSConfig.ftp)
                this.zFtpList.Items.Add(f);

            this.zImageWidth.Text = CMSConfig.imagewidth.ToString();
            this.zImageHeight.Text = CMSConfig.imageheight.ToString();
            this.zImageQuality.Text = CMSConfig.imagequality.ToString();
            
        }

        private void loadDefaultConfig()
        {
            this.zBroadcastDelay.Text = "15";
            this.zSlideCartPrefix.Text = "vis-";
            this.zTemplateDir.Text = "data\\templates";
            this.zSlideDir.Text = "data\\slides";
            this.zWatchDir.Text = "";
            this.zHttpPort.Text = "8081";
            this.zHttpUrlSeparator.Text = "|";
            this.zRadioVisLink.Text = "http://tech.ebu.ch/";
            this.zRadioVisText.Text = "CONTENT MANAGER DEMO";
            this.zStompHttpUrl.Text = "http://www.ebulabs.org/cms/uploaddir/";
            this.zStompLogin.Text = "";
            this.zStompPasscode.Text = "";
            this.zStompServer.Text = "radiodns1.ebu.ch";
            this.zStompPort.Text = "61613";

            this.zImageWidth.Text = "320";
            this.zImageHeight.Text = "240";
            this.zImageQuality.Text = "100";
         
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Boolean valid = false;
            if (nFtpHost.Text == "" || !nFtpHost.Text.StartsWith("ftp://"))
                MessageBox.Show("FTP host must be defined like : ftp://myserver/folder/");
            else if (nFtpLogin.Text == "")
                MessageBox.Show("FTP login must be defined");
            else if (nFtpPassword.Text == "")
                MessageBox.Show("FTP password must be defined");
            else
                valid = true;

            int minperiod = 0;
            try
            {
                minperiod = Int32.Parse(nFtpMinPeriod.Text);
            }
            catch
            {
                MessageBox.Show("FTP minperiod must be an Integer");
                valid = false;
            }
            if (valid)
            {
                String host = nFtpHost.Text;
                String login = nFtpLogin.Text;
                String password = nFtpPassword.Text;
                
                FtpAccount f = new FtpAccount();
                f.server = host;
                f.login = login;
                f.password = password;
                f.minperiod = minperiod;

                this.zFtpList.Items.Add(f);
            }
        }

        private Boolean saveConfig()
        {
            int broadcastdelay = 0;
            try
            {
                broadcastdelay = Int32.Parse(this.zBroadcastDelay.Text);
            }
            catch
            {
                MessageBox.Show("BroadcastDelay must be an integer");
                return false;
            }
            String slidecartprefix=this.zSlideCartPrefix.Text;
            String dirtemplate = this.zTemplateDir.Text;
            String dirslide = this.zSlideDir.Text;
            String dirwatch = this.zWatchDir.Text;
            int httpport = 8081;
            try
            {
                httpport = Int32.Parse(this.zHttpPort.Text);
            }
            catch
            {
                MessageBox.Show("HTTP port must be an integer");
                return false;
            }

            String httpurlseparator=this.zHttpUrlSeparator.Text;
            String radiovislink= this.zRadioVisLink.Text;
            String radiovistxt=this.zRadioVisText.Text;
            String stomphttpurl=this.zStompHttpUrl.Text;
            String stomplogin = this.zStompLogin.Text;
            String stomppasscode=this.zStompPasscode.Text;
            String stompserver = this.zStompServer.Text;
            int stompport = Int32.Parse(this.zStompPort.Text);

            LinkedList<String> stomptopic = new LinkedList<String>();
            stomptopic.Clear();
            foreach (String s in this.zStompTopicsList.Items)
                stomptopic.AddFirst(s);
            

            String inputtype = this.zTypeList.Text.ToUpper();
            LinkedList<FtpAccount> ftp = new LinkedList<FtpAccount>();
            ftp.Clear();
            foreach (FtpAccount f in this.zFtpList.Items)
                ftp.AddFirst(f);

            int imagewidth = 320;
            int imageheight =240;
            try
            {
                imagewidth = Int32.Parse(this.zImageWidth.Text);
                imageheight = Int32.Parse(this.zImageHeight.Text);
            }
            catch{
                MessageBox.Show("Image Size must be integers");
                return false;
            }

            int q = 100;
            int imagequality = 100;
            try
            {
                q = Int32.Parse(this.zImageQuality.Text);

                if (q < 0 || q > 100)
                    MessageBox.Show("Image Quality must be defined between 0 and 100");
                else
                    imagequality = q;
            }
            catch
            {
                MessageBox.Show("Image Quality must be defined between 0 and 100");
                return false;
            }
            /** everything is checked, we can save it to file.. */
            CMSConfig.broadcastdelay = broadcastdelay;
            CMSConfig.slidecartprefix = slidecartprefix;
            CMSConfig.dirtemplate = dirtemplate;
            CMSConfig.dirslide = dirslide;
            CMSConfig.dirwatch = dirwatch;
            CMSConfig.httpport = httpport;


            CMSConfig.httpurlseparator = httpurlseparator;
            CMSConfig.radiovislink = radiovislink;
            CMSConfig.radiovistxt = radiovistxt;
            CMSConfig.stomphttpurl = stomphttpurl;
            CMSConfig.stomplogin = stomplogin;
            CMSConfig.stomppasscode = stomppasscode;
            CMSConfig.stompserver = stompserver;
            CMSConfig.stompport = stompport;
            CMSConfig.stomptopic.Clear();
            foreach (String s in stomptopic)
                CMSConfig.stomptopic.AddFirst(s);


            CMSConfig.inputtype = inputtype;
            CMSConfig.ftp.Clear();
            foreach (FtpAccount f in ftp)
                CMSConfig.ftp.AddFirst(f);

            CMSConfig.imagewidth = imagewidth;
            CMSConfig.imageheight = imageheight;

            CMSConfig.imagequality = imagequality;

            CMSConfig.save(cf);

            return true;
        

            
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            int ind = zFtpList.SelectedIndex;
            if (ind != -1)
                this.zFtpList.Items.RemoveAt(ind);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("After modification, the application must restart.\nWould you like to proceed?", "Restart", MessageBoxButton.OKCancel);
            if(r==MessageBoxResult.OK){

                if (saveConfig())
                {
                    if(UIMain.core != null)
                        UIMain.core.close();
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Environment.Exit(0);
                }

               
            }
            else{
            
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "XML File|*.xml";
            open.ShowDialog();

            if (open.FileName != "")
            {
                File.Copy(open.FileName,cf,true);

                if (UIMain.core != null)
                    UIMain.core.close();
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Environment.Exit(0);
            }

            

                

            

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.OverwritePrompt = true;
            sfd.Title = "export";
            sfd.AddExtension = true;
            sfd.DefaultExt = "*.xml";
            //sfd.Filter="Config File|config.xml|Xml File|*.xml|All Files|*.*";
            sfd.Filter = "XML File|*.xml";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                File.Copy(cf, sfd.FileName, true);
            }
        }

        private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem != null)
                nStompTopic.Text = ((ListBox)sender).SelectedItem.ToString();
        }

        private void TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.exportbtn.IsEnabled = false;
        }

    }
}