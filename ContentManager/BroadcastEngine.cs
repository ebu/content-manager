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
using System.Windows.Threading;
using SlideGeneratorLib;
using System.Windows.Controls;
using System.Threading;
using ContentManager.Output.Stomp;
using ContentManager.Output.Ftp;
using System.Net;
using System.IO;
using Apache.NMS;
using Apache.NMS.Util;
using ContentManager.GUI;
using SlideGeneratorLib.Parser;
using System.Windows;
using XMLConfig.CMS;

namespace ContentManager
{
    public class BroadcastEngine
    {

        public int broadcastperiod { get; private set; } //in seconds
        private DispatcherTimer timer { get; set; }

        public List<String> slidecart { get; set; }
        public int slidePosition = 0;
        public SlideGenerator slidegen { get; set; }

        public String slideOnAir = "";

        private OutputFtp outputFtp;
        private OutputStomp outputStomp;


        public delegate void OutputEvent(String filename, String link);
        public event OutputEvent onBroadcast;


        public delegate void ChangeCartEvent(List<String> slides);
        public event ChangeCartEvent onChangeCart;

        public void setSlideCart(List<string> slidenames){
            this.slidecart.Clear();
            for(int i=0;i<slidenames.Count;i++){
                this.slidecart.Add(slidenames[i]);
            }
            if (this.slidecart.Count == 0)
                this.stopAutoBroadcast();

            if(this.onChangeCart!=null)
                 this.onChangeCart(slidenames);
        }

        public BroadcastEngine(SlideGenerator slidegen)
        {
            this.slidegen = slidegen;
            this.slidecart = new List<string>();

            try
            {
                this.broadcastperiod = CMSConfig.broadcastdelay;
            }
            catch
            {
                this.broadcastperiod = 10;
            }

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(broadcastperiod);
            this.timer.Tick += new EventHandler(timer_Tick);

            this.outputStomp = new OutputStomp(this.slidegen);
            this.outputFtp = new OutputFtp();
            this.outputFtp.onUploadEnd += this.outputStomp.sendToStompProcess;
            this.outputFtp.onUploadEnd += new OutputFtp.OutputEvent(outputFtp_onUploadEnd);

            timer_Tick(this, new EventArgs());
        }

        void outputFtp_onUploadEnd(string filename, string link)
        {
            this.onBroadcast(filename, link);
        }

        int j = 0;
        int lastindex = 0;
        void timer_Tick(object sender, EventArgs e)
        {
 
         /*   if (j == 0 && this.slidegen.cstlist.ContainsKey("BSLIDE") && this.slidegen.cstlist["BSLIDE"] != "" && (this.slidegen.cstlist["BSLIDE"]!= this.slideOnAir || this.slidecart.Count==0))
            {
                if(this.slidegen.cstlist["BSLIDE"]!= this.slideOnAir)
                    this.lastindex = this.slidecart.IndexOf(this.slideOnAir);
                broadcast(this.slidegen.cstlist["BSLIDE"]);
            }
            else
            {*/
            broadcastNextInCart();
         /*   }
            j = (j + 1) % 5;*/
        }

        public void broadcastNextInCart()
        {
            int i = this.slidecart.IndexOf(this.slideOnAir);
            if (i == -1) i = 0;// lastindex;
            else if (i + 1 == this.slidecart.Count) i = 0;
            else i++;
            if (this.slidecart.Count > 0)
                broadcast(this.slidecart.ElementAt(i));
        }


        void start()
        {
            UIMain.timerValue = 0;
            this.timer.Start();
            
            
        }

        void stop()
        {
            this.timer.Stop();
        }

        internal String preview(String slidename, Dictionary<String,String> dic=null)
        {

            String filename = DateTime.Now.ToFileTimeUtc() + "-d-" + slidename + ".jpg";
            UIMain.Instance.Dispatcher.Invoke((Action)delegate()
            {
                SlideResult slideres = slidegen.loadXMLSlide(slidename, dic);
            
            if (slideres != null)
            {
                Canvas slide = slideres.image;
                
                int quality = CMSConfig.imagequality;
                slidegen.saveToJpg(slide, SlideGenerator.tmpfolder + filename, "nolink", quality);
           


                
            }
            else
            {
                UIMain.errorAdd("[SLIDE PREVIEW] Not found or error when loading slide " + filename);
            }
            });
           
            return filename;
        }

        internal String broadcast(String slidename)
        {

            
           this.resetTimer();
            
            this.slideOnAir = slidename;
            
            
            String filename = DateTime.Now.ToFileTimeUtc() + "-d-" + slidename + ".jpg";
            
            SlideResult slideres = slidegen.loadXMLSlide(slidename);
            ///BUG -> handle : slideres == null
            try
            {
                this.setInterval(TimeSpan.FromSeconds(slideres.broadcastdelay));
            }
            catch (Exception e)
            {
                return "";
            }

            if (slideres != null)
            {
                Canvas slide = slideres.image;
                String link = VarParser.parseText(slideres.link, this.slidegen.cstlist, null);
                
                if (link.Equals(""))
                {
                    try
                    {
                        link = slidegen.cstlist["RADIOVISLINK"];
                    }
                    catch { }
                    if (link.Equals(""))
                        link = CMSConfig.radiovislink;
                }
                
                slidegen.saveToJpg(slide, SlideGenerator.tmpfolder + filename, link, CMSConfig.imagequality);
                
                outputFtp.send(filename, SlideGenerator.tmpfolder, link);
                UIMain.errorAdd("[SLIDE] " + filename + " broadcasted...");

            }
            else
            {
                UIMain.errorAdd("[SLIDE] Not found or error when loading slide " + filename);
            }
            
           // this.onBroadcast(this, new EventBroadcastArgs(slidekey, DateTime.Now.ToLongTimeString()));
            return Path.GetFullPath(SlideGenerator.tmpfolder +filename);
        }

        private void resetTimer()
        {
            this.timer.Stop();
            this.timer.Start();
        }


        internal void startAutoBroadcast()
        {
            this.timer_Tick(null, new EventArgs());
            this.timer.Start();
        }

        internal void stopAutoBroadcast()
        {   
            this.timer.Stop();
        }

        internal Boolean isAutoBroadcastEnabled()
        {
            return this.timer.IsEnabled;
        }

        internal void setInterval(TimeSpan Interval)
        {
            this.stop();
            this.timer.Interval = Interval;
            this.start();
        }
        internal TimeSpan getInterval()
        {
            return this.timer.Interval;
        }

    }
}
