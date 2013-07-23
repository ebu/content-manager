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
using SlideGeneratorLib;
using ContentManager.Input.HTTP;
using System.Windows;
using ContentManager.GUI;
using System.Deployment.Application;
using System.IO;
using XMLConfig.CMS;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using CsvHelper;
using CsvHelper.Configuration;
using System.Windows.Threading;
using log4net;
using log4net.Config;
using log4net.Appender;
using log4net.Layout;

namespace ContentManager
{
    /// <summary>
    /// SINGLETON
    /// 
    /// </summary>
    public class ContentManagerCore
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ContentManagerCore));

        private static ContentManagerCore instance = null;
        public SlideGenerator slidegen;
        public BroadcastEngine engine;
        public AMQPEngine amqpEngine;

        public String datafolder;
        public String tmpfolder;

        public IInputPlugin input;

        public static Boolean debug = false;

        //Output


        private static Object InstanceLock = new Object();

        private ContentManagerCore()
        {
            ConsoleAppender appender = new ConsoleAppender();
            PatternLayout pl = new PatternLayout();
            pl.ConversionPattern = "%d [%2%t] %-5p [%-10c]   %m%n%n";
            pl.ActivateOptions();
            appender.Layout = pl;
            BasicConfigurator.Configure(appender);

            //XmlConfigurator.Configure(new System.IO.FileInfo("logger.config"));
            logger.Info("Configure Logger");
            try
            {
                debug = System.Configuration.ConfigurationManager.AppSettings["debug"].Equals("true");


                datafolder = System.Configuration.ConfigurationManager.AppSettings["DataFolder"];
                tmpfolder = System.Configuration.ConfigurationManager.AppSettings["TmpFolder"];


                if (!Directory.Exists(datafolder) && !File.Exists(datafolder + "\\config.xml") && ApplicationDeployment.IsNetworkDeployed)
                    datafolder = ApplicationDeployment.CurrentDeployment.DataDirectory + "\\" + datafolder;

                if (!Directory.Exists(tmpfolder) && ApplicationDeployment.IsNetworkDeployed)
                    tmpfolder = ApplicationDeployment.CurrentDeployment.DataDirectory + "\\" + tmpfolder;

                try
                {

                    if (!Directory.Exists(tmpfolder))
                        Directory.CreateDirectory(tmpfolder);

                    this.slidegen = new SlideGenerator(datafolder + "\\config.xml", datafolder + "\\variables.xml", datafolder, tmpfolder);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    Environment.Exit(1);
                }


                try
                {
                    if (CMSConfig.inputtype.ToUpper() == "HTTP")
                        this.input = new InputHTTPKayak(this);
                    else if (CMSConfig.inputtype.ToUpper() == "WCF")
                        this.input = new InputWCF(this);
                    else
                    {
                        MessageBox.Show("There is no InputMethod key in the configuration file", "Error: Bad Configuration");
                        Environment.Exit(1);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

                this.engine = new BroadcastEngine(slidegen);
                this.engine.setSlideCart(this.getAutoSlides());
                if (this.engine.slidecart.Count != 0)
                    this.engine.startAutoBroadcast();

                amqpEngine = new AMQPEngine();
                amqpEngine.onEvent += new AMQPEngine.Event(amqpEngine_onEvent);
                amqpEngine.start();

            }
            catch (Exception e)
            {
                MessageBox.Show("Error when starting: " + e.Message);
            }
        }

        void amqpEngine_onEvent(Newtonsoft.Json.Linq.JObject jsonObject)
        {
            if (jsonObject["type"].ToString().Equals("StartList"))
            {
                logger.Info("New StartList notification");

                var compNames = jsonObject["competitors"]["long_tv_name"];
                var compCountries = jsonObject["competitors"]["noc_code"];
                var compLanes = jsonObject["competitors"]["lane"];


                for (int i = 0; i < compNames.Count(); i++)
                {
                    UIMain.core.slidegen.setVar("SW_NAME" + compLanes[i].ToString(), compNames[i].ToString());
                    UIMain.core.slidegen.setVar("SW_NOC" + compLanes[i].ToString(), compCountries[i].ToString());
                    UIMain.core.slidegen.setVar("SW_LANE" + compLanes[i].ToString(), compLanes[i].ToString());
                }

                try
                {
                    var eventName = jsonObject["context"]["event"];
                    UIMain.core.slidegen.setVar("SW_CTX_EVENT", eventName.ToString());
                    var competition = jsonObject["context"]["competition"];
                    UIMain.core.slidegen.setVar("SW_CTX_COMPETITION", competition.ToString());
                    var eventUnit = jsonObject["context"]["eventUnit"];
                    UIMain.core.slidegen.setVar("SW_CTX_EVENTUNIT", eventUnit.ToString());


                    UIMain.updateSwimmingStatus(jsonObject["context"]["event"].ToString(), "START LIST");

                }
                catch (Exception e) { logger.Error("Uncomplete notification", e); }

                List<String> l = new List<string>();
                l.Add("sw-startlist-1");
                l.Add("sw-startlist-2");
                UIMain.core.engine.setSlideCart(l);
                UIMain.core.broadcast("sw-startlist-1");

            }
            else if (jsonObject["type"].ToString().Equals("ArrivalList"))
            {
                logger.Info("New ArrivalList notification");

                var competitors = jsonObject["competitors"];

                resetCompetitors();


                for (int i = 1; i <= competitors.Count(); i++)
                {
                    UIMain.core.slidegen.setVar("SW_NAME" + (i - 1), competitors[i.ToString()]["long_tv_name"].ToString());
                    UIMain.core.slidegen.setVar("SW_NOC" + (i - 1), competitors[i.ToString()]["noc_code"].ToString());
                    UIMain.core.slidegen.setVar("SW_TIME" + (i - 1), competitors[i.ToString()]["time"].ToString());
                }

                try
                {
                    var eventName = jsonObject["context"]["event"];
                    UIMain.core.slidegen.setVar("SW_CTX_EVENT", eventName.ToString());
                    var competition = jsonObject["context"]["competition"];
                    UIMain.core.slidegen.setVar("SW_CTX_COMPETITION", competition.ToString());
                    var eventUnit = jsonObject["context"]["eventUnit"];
                    UIMain.core.slidegen.setVar("SW_CTX_EVENTUNIT", eventUnit.ToString());


                    UIMain.updateSwimmingStatus(jsonObject["context"]["event"].ToString(), "ARRIVAL LIST");

                }
                catch (Exception e) { logger.Error("Uncomplete notification", e); }


                List<String> l = new List<string>();
                l.Add("sw-arrivallist");
                UIMain.core.engine.setSlideCart(l);
                UIMain.core.broadcast("sw-arrivallist");

            }
            else if (jsonObject["type"].ToString().Equals("FinalList"))
            {
                logger.Info("New FinalList notification");

                var competitors = jsonObject["competitors"];


                for (int i = 1; i <= competitors.Count(); i++)
                {
                    if (competitors[i.ToString()] != null)
                    {
                        UIMain.core.slidegen.setVar("SW_NAME" + i, competitors[i.ToString()]["long_tv_name"].ToString());
                        UIMain.core.slidegen.setVar("SW_NOC" + i, competitors[i.ToString()]["noc_code"].ToString());
                        UIMain.core.slidegen.setVar("SW_TIME" + i, competitors[i.ToString()]["time"].ToString());
                    }
                    else
                    {

                        UIMain.core.slidegen.setVar("SW_NAME" + i, "");
                        UIMain.core.slidegen.setVar("SW_NOC" + i, "");
                        UIMain.core.slidegen.setVar("SW_TIME" + i, "");
                    }
                }

                try
                {
                    var eventName = jsonObject["context"]["event"];
                    UIMain.core.slidegen.setVar("SW_CTX_EVENT", eventName.ToString());
                    var competition = jsonObject["context"]["competition"];
                    UIMain.core.slidegen.setVar("SW_CTX_COMPETITION", competition.ToString());
                    var eventUnit = jsonObject["context"]["eventUnit"];
                    UIMain.core.slidegen.setVar("SW_CTX_EVENTUNIT", eventUnit.ToString());


                    UIMain.updateSwimmingStatus(jsonObject["context"]["event"].ToString(), "RESULTS");
                }
                catch (Exception e) { logger.Error("Uncomplete notification", e); }

                List<String> l = new List<string>();
                l.Add("sw-finallist-1");
                l.Add("sw-finallist-2");
                UIMain.core.engine.setSlideCart(l);
                UIMain.core.broadcast("sw-finallist-1");



            }

            else if (jsonObject["type"].ToString().Equals("Standby"))
            {
                logger.Info("New Standby notification");


                List<String> l = new List<string>();
                l.Add("logo");
                l.Add("twitter");
                UIMain.core.engine.setSlideCart(l);
                UIMain.core.broadcast("logo");

            }


        }

        private void resetCompetitors()
        {
            for (int i = 0; i < 10; i++)
            {
                UIMain.core.slidegen.setVar("SW_NAME" + i, "");
                UIMain.core.slidegen.setVar("SW_NOC" + i, "");
                UIMain.core.slidegen.setVar("SW_TIME" + i, "");
                UIMain.core.slidegen.setVar("SW_LANE" + i, "");
            }
        }


        public static ContentManagerCore getInstance()
        {
            lock (InstanceLock)
            {
                if (instance == null)
                {
                    instance = new ContentManagerCore();
                }
                return instance;
            }
        }

        internal void test()
        {
            /*SlideResult slideres = this.slidegen.loadXMLSlide("u2");
            if(slideres != null)
            this.slidegen.saveToJpg(slideres.image, "test.jpg", true);*/
        }

        internal void update(Dictionary<string, string> dictionary)
        {
            //Lock for enumeration thread safety
            lock (slidegen.cstlist)
            {
                if (dictionary != null)
                {
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        if (slidegen.cstlist.ContainsKey(dictionary.ElementAt(i).Key))
                        {
                            slidegen.cstlist[dictionary.ElementAt(i).Key] = dictionary.ElementAt(i).Value;
                            if (dictionary.ElementAt(i).Key == "CURRENTARTIST")
                                slidegen.cstlist["ARTIST"] = dictionary.ElementAt(i).Value;
                            if (dictionary.ElementAt(i).Key == "CURRENTTITLE")
                                slidegen.cstlist["TITLE"] = dictionary.ElementAt(i).Value;
                        }
                        else
                        {
                            slidegen.cstlist.Add(dictionary.ElementAt(i).Key, dictionary.ElementAt(i).Value);

                            if (dictionary.ElementAt(i).Key == "CURRENTARTIST")
                                slidegen.cstlist.Add("ARTIST", dictionary.ElementAt(i).Value);
                            if (dictionary.ElementAt(i).Key == "CURRENTTITLE")
                                slidegen.cstlist.Add("TITLE", dictionary.ElementAt(i).Value);
                        }
                    }
                }
            }
        }

        internal String broadcast(String slidename)
        {
            String filename = "";
            UIMain.Instance.Dispatcher.Invoke((Action)delegate() { filename = this.engine.broadcast(slidename); });
            return filename;
        }

        private Boolean isSlideCartValid()
        {
            List<String> slide = slidegen.getAvailableSlides();

            if (this.engine.slidecart.Count == 0)
                return false;

            foreach (String s in this.engine.slidecart)
            {
                if (!slide.Contains(s))
                    return false;
            }
            return true;
        }

        public void checkAfterChangeDir()
        {

            if (!isSlideCartValid())
            {
                Console.WriteLine("CART NOT VALID");

                this.engine.setSlideCart(this.getAutoSlides());
                UIMain.Instance.Dispatcher.Invoke((Action)delegate()
                {
                    if (this.engine.slidecart.Count != 0)
                        this.engine.startAutoBroadcast();
                    else
                        this.engine.stopAutoBroadcast();
                });
            }
            else
                Console.WriteLine("CART VALID");

        }

        public List<String> getAutoSlides()
        {
            List<String> slides = new List<String>();
            List<String> availableSlides = this.slidegen.getAvailableSlides();
            for (int i = 0; i < availableSlides.Count; i++)
            {
                String slidename = availableSlides.ElementAt(i);
                if (slidename.IndexOf(CMSConfig.slidecartprefix) == 0)
                {
                    slides.Add(slidename);
                    Console.WriteLine("AUTOSLIDE : " + slidename);
                }

            }
            return slides;
        }

        internal void close()
        {
            this.input.stop();
        }
    }
}
