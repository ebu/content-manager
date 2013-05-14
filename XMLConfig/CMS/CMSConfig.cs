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
using System.Xml;
using System.IO;

namespace XMLConfig.CMS
{
    public class CMSConfig
    {
        public static String configfile = "";
        public static String dirslide = "";
        public static String dirtemplate = "";
        
        public static String inputtype = "";
        public static int httpport = 80;

        public static String httpurlseparator = "&";
        public static int broadcastdelay = 15;
        public static String radiovislink = "";
        public static String radiovistxt = "";

        public static String stompserver = "";
        public static int stompport = 61613;
        public static String stomphttpurl = "";
        public static String stomplogin = "";
        public static String stomppasscode = "";
        public static LinkedList<String> stomptopic = new LinkedList<String>();
        public static LinkedList<FtpAccount> ftp = new LinkedList<FtpAccount>();
        public static int imagequality = 100;

        public static String slidecartprefix = "vis-";

        public static int imagewidth = 320;
        public static int imageheight = 240;

        public static String dirwatch = "";
        public static int ep_quality=50;
        public static String stationimage = "";


        // GLOBAL VARIABLES
        public static String video_tmpfolder = "tmp-video";

        public static void print()
        {

            Console.WriteLine("Current config:");
            Console.WriteLine("dirslide = " + dirslide);
            Console.WriteLine("dirtemplate = " + dirtemplate);
            Console.WriteLine("dirwatch = " + dirwatch);
            Console.WriteLine();
            Console.WriteLine("inputtype = " + inputtype);
            Console.WriteLine("httpport = " + httpport);
            Console.WriteLine("httpurlseparator = " + httpurlseparator);
            Console.WriteLine();
            Console.WriteLine("broadcastdelay = " + broadcastdelay);
            Console.WriteLine("radiovislink = " + radiovislink);
            Console.WriteLine("radiovistxt = " + radiovistxt);
            Console.WriteLine();
            Console.WriteLine("stompServer = " + stompserver);
            Console.WriteLine("stompPort = " + stompport);
            Console.WriteLine("stompHttpUrl = " + stomphttpurl);
            Console.WriteLine("stompLogin = " + stomplogin);
            Console.WriteLine("stompPasscode = " + stomppasscode);
            Console.WriteLine();
            Console.WriteLine("imagewidth = " + imagewidth);
            Console.WriteLine("imageheight = " + imageheight);
            Console.WriteLine("imagequality = " + imagequality);
            Console.WriteLine("ep_quality = " + ep_quality);

            Console.WriteLine("stationimage = " + stationimage);
        
            Console.WriteLine();
            
            

            Console.Write(stomptopic.Count + " stomptopic = {");
            for (int i = 0; i < stomptopic.Count; i++)
            {
                Console.Write(stomptopic.ElementAt(i));
                if (i != stomptopic.Count - 1)
                    Console.Write(", ");
            }
            Console.WriteLine("}");
            Console.WriteLine();
            for (int i = 0; i < ftp.Count; i++)
                ftp.ElementAt(i).print();
            
            Console.WriteLine("slidecartprefix = " + slidecartprefix);

        }

        public static void load(String filename) 
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException();
            configfile = filename;
            try
            {
                XmlTextReader reader = new XmlTextReader(filename);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.

                            if (reader.Name == "slide")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "src")
                                        CMSConfig.dirslide = reader.Value;

                                }
                            }
                            else if (reader.Name == "template")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "src")
                                        CMSConfig.dirtemplate = reader.Value;

                                }
                            }
                            else if (reader.Name == "watch")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "src")
                                        CMSConfig.dirwatch = reader.Value;

                                }
                            }
                            else if (reader.Name == "input")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "type")
                                        CMSConfig.inputtype = reader.Value;
                                    else if (reader.Name == "httpport")
                                        CMSConfig.httpport = Int32.Parse(reader.Value);
                                    else if (reader.Name == "httpurlseparator")
                                        CMSConfig.httpurlseparator = reader.Value;

                                }

                            }
                            else if (reader.Name == "ftp")
                            {
                                FtpAccount f = new FtpAccount();
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "server")
                                        f.server = reader.Value;
                                    else if (reader.Name == "user")
                                        f.login = reader.Value;
                                    else if (reader.Name == "password")
                                        f.password = reader.Value;
                                    else if (reader.Name == "minperiod")
                                        f.minperiod = Int32.Parse(reader.Value);
                                    else if (reader.Name == "externalprocess")
                                        f.externalProcess = (reader.Value.Equals("yes"));

                                }
                                CMSConfig.ftp.AddLast(f);

                            }

                            else if (reader.Name == "stomp")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "server")
                                        CMSConfig.stompserver = reader.Value;
                                    else if (reader.Name == "port")
                                        CMSConfig.stompport = Int32.Parse(reader.Value);
                                    else if (reader.Name == "login")
                                        CMSConfig.stomplogin = reader.Value;
                                    else if (reader.Name == "passcode")
                                        CMSConfig.stomppasscode = reader.Value;
                                    else if (reader.Name == "httpurl")
                                        CMSConfig.stomphttpurl = reader.Value;
                                }
                            }
                            else if (reader.Name == "topic")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "destination")
                                        CMSConfig.stomptopic.AddLast(reader.Value);
                                }
                            }
                            else if (reader.Name == "playout")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "broadcastdelay")
                                        CMSConfig.broadcastdelay = Int32.Parse(reader.Value);
                                    else if (reader.Name == "slidecartprefix")
                                        CMSConfig.slidecartprefix = reader.Value;
                                }
                            }
                            else if (reader.Name == "radiovis")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "link")
                                        CMSConfig.radiovislink = reader.Value;
                                    else if (reader.Name == "text")
                                        CMSConfig.radiovistxt = reader.Value;
                                }
                            }
                            else if (reader.Name == "image")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "width")
                                        CMSConfig.imagewidth = Int32.Parse(reader.Value);
                                    else if (reader.Name == "height")
                                        CMSConfig.imageheight = Int32.Parse(reader.Value);
                                    else if (reader.Name == "quality")
                                        CMSConfig.imagequality = Int32.Parse(reader.Value);
                                    else if (reader.Name == "ep_quality")
                                        CMSConfig.ep_quality = Int32.Parse(reader.Value);
                                }
                            }
                            else if (reader.Name == "station")
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name == "image")
                                    {
                                        CMSConfig.stationimage = reader.Value;
                                    }
                                }
                            }
                            break;
                        case XmlNodeType.Text: //Display the text in each element.

                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.

                            break;
                    }
                }


                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: "+e.Message);
            }

            CMSConfig.print();


        }



        public static void save(String filename)
        {
            String dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (XmlTextWriter w = new XmlTextWriter(filename, Encoding.UTF8))
            {

                w.Formatting = Formatting.Indented;
                w.WriteStartDocument();

                w.WriteStartElement("config");

                w.WriteStartElement("dir");
                w.WriteStartElement("template");
                w.WriteStartAttribute("src");
                w.WriteValue(CMSConfig.dirtemplate);
                w.WriteEndAttribute();
                w.WriteEndElement();

                w.WriteStartElement("watch");
                w.WriteStartAttribute("src");
                w.WriteValue(CMSConfig.dirwatch);
                w.WriteEndAttribute();
                w.WriteEndElement();

                w.WriteStartElement("slide");
                w.WriteStartAttribute("src");
                w.WriteValue(CMSConfig.dirslide);
                w.WriteEndAttribute();
                w.WriteEndElement();
                w.WriteEndElement();
                
                w.WriteStartElement("input");
                w.WriteStartAttribute("type");
                w.WriteValue(CMSConfig.inputtype);
                w.WriteStartAttribute("httpport");
                w.WriteValue(CMSConfig.httpport);
                w.WriteStartAttribute("httpurlseparator");
                w.WriteValue(CMSConfig.httpurlseparator);
                w.WriteEndElement();
                w.WriteStartElement("playout");
                w.WriteStartAttribute("broadcastdelay");
                w.WriteValue(CMSConfig.broadcastdelay);
                w.WriteStartAttribute("slidecartprefix");
                w.WriteValue(CMSConfig.slidecartprefix);
                w.WriteEndElement();
                w.WriteStartElement("radiovis");
                w.WriteStartAttribute("link");
                w.WriteValue(CMSConfig.radiovislink);
                w.WriteStartAttribute("text");
                w.WriteValue(CMSConfig.radiovistxt);
                w.WriteEndElement();
                w.WriteStartElement("render");
                w.WriteStartElement("image");
                w.WriteStartAttribute("width");
                w.WriteValue(CMSConfig.imagewidth);
                w.WriteStartAttribute("height");
                w.WriteValue(CMSConfig.imageheight);
                w.WriteStartAttribute("quality");
                w.WriteValue(CMSConfig.imagequality);
                w.WriteStartAttribute("ep_quality");
                w.WriteValue(CMSConfig.ep_quality);
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("stomp");
                w.WriteStartAttribute("server");
                w.WriteValue(CMSConfig.stompserver);
                w.WriteStartAttribute("port");
                w.WriteValue(CMSConfig.stompport);
                w.WriteStartAttribute("login");
                w.WriteValue(CMSConfig.stomplogin);
                w.WriteStartAttribute("passcode");
                w.WriteValue(CMSConfig.stomppasscode);
                w.WriteStartAttribute("httpurl");
                w.WriteValue(CMSConfig.stomphttpurl);
                for (int i = 0; i < CMSConfig.stomptopic.Count; i++)
                {
                    w.WriteStartElement("topic");
                    w.WriteStartAttribute("destination");
                    w.WriteValue(CMSConfig.stomptopic.ElementAt(i));
                    w.WriteEndElement();
                }
                w.WriteEndElement();

                foreach (FtpAccount f in CMSConfig.ftp)
                {
                    if (!f.server.EndsWith("/"))
                        f.server = f.server + "/";
                    w.WriteStartElement("ftp");
                    w.WriteStartAttribute("server");
                    w.WriteValue(f.server);
                    w.WriteStartAttribute("user");
                    w.WriteValue(f.login);
                    w.WriteStartAttribute("password");
                    w.WriteValue(f.password);
                    w.WriteStartAttribute("minperiod");
                    w.WriteValue(f.minperiod);
                    w.WriteStartAttribute("externalprocess");
                    w.WriteValue(((f.externalProcess)?"yes":"no"));
                    w.WriteEndElement();

                }

                w.WriteStartElement("station");
                w.WriteStartAttribute("image");
                w.WriteValue(CMSConfig.stationimage);
                w.WriteEndElement();


                w.WriteEndDocument();
                w.Flush();

                w.Close();
            }

        }

    }
}
