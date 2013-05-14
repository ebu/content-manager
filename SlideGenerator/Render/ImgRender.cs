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
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.IO;
using SlideGeneratorLib.Parser;
using System.Threading;
using System.Net;
using System.Deployment.Application;

namespace SlideGeneratorLib.Rendering
{
    class ImgRender:ARender
    {
        public ImgRender(Dictionary<string, string> dic) : base("img") { this.dictionary = dic; }
        private Dictionary<string, string> dictionary;

        public override void draw(XElement field, Canvas c)
        {
            draw(field, c, null);
        }
        public override void draw(XElement field, Canvas c, Dictionary<String,String> localdic)    
        {
            DateTime n = DateTime.Now;
            if (field.Attribute("src") != null)
            {
                System.Windows.Controls.Image box = new System.Windows.Controls.Image();
                RenderOptions.SetBitmapScalingMode(box, BitmapScalingMode.Fant);
                String path = VarParser.parseText(field.Attribute("src").Value, dictionary, localdic);
                String exactpath = "";
                if (path.StartsWith("http://"))
                    exactpath = fileexists(path);
                else if (File.Exists(path))
                    exactpath= fileexists(path);
                else
                    exactpath = fileexists(SlideGenerator.datafolder + "\\" + path);
                Console.WriteLine("PATH : --- " + exactpath);
                if (exactpath != "-1")
                {
                    Console.WriteLine("FILE OK");
                    try
                    {
                        Console.WriteLine("IMG: " + exactpath + " path:" + path);
                        if (exactpath.StartsWith("http://"))
                        {
                            String filename=SlideGenerator.tmpfolder + "test-"+DateTime.Now.ToFileTime()+".jpg";
                            try
                            {
                                WebClient Client = new WebClient();
                                
                                Client.DownloadFile(new Uri(exactpath), filename);
                                Boolean ok = false;
                                while (!ok)
                                {
                                    try
                                    {
                                        Uri u = new Uri(filename, UriKind.RelativeOrAbsolute);
                                        BitmapImage i = new BitmapImage(u);
                                        

                                        box.Source = i;
                                        
                                        box.Stretch = System.Windows.Media.Stretch.Fill;
                                        
                                        
                                        addToCanvas(field, box, c);
                                        File.Delete(filename);
                                        Console.WriteLine("ok");
                                        ok = true;
                                    }
                                    catch(Exception e)
                                    {
                                     if(!e.Message.EndsWith("because it is being used by another process."))
                                         ok = true;
                                         Console.WriteLine("Error downloading file " + exactpath + " to " + filename);
                                         Console.WriteLine(e.Message);
                                    }

                                }
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Error downloading file " + exactpath + " to " + filename);
                                Console.WriteLine(e.Message);
                            }


                        }
                        else
                        {

                            Uri u = new Uri(exactpath, UriKind.RelativeOrAbsolute);
                            BitmapImage i = new BitmapImage(u);
                            box.Source = i;

                            box.Stretch = System.Windows.Media.Stretch.Fill;

                            addToCanvas(field, box, c);
                        }
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[IMG TAG ERROR] Unable to load src={" + exactpath + "}");
                    }
                }
                else
                {
                    Console.WriteLine("File " + path + " not found!!!");
                }


                Console.WriteLine("Component " + field.Value + " generated in " + DateTime.Now.Subtract(n).TotalMilliseconds + "ms");
            }
        }

        private String fileexists(string path)
        {
            Console.WriteLine("** " + path);
            if (path.IndexOf("http://") == 0)
            {
                if (path.Contains("?"))
                    return path;
                else
                    return path + "?" + DateTime.Now.ToFileTime();
            }


            try
            {
                String file;

              /*  if (!ApplicationDeployment.IsNetworkDeployed)
                {
                    String[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(path));
                    //String path2 = System.IO.Path.GetFullPath(path);
                    file = Array.Find(files, s => (s.ToLower() == path.ToLower()));
                    
                }
                else
                {*/
                file = System.IO.Path.GetFullPath(path);
              //  }
                if (file != "" && file != null) return file;
                else return "-1";
            }
            catch {

/*                //try deployement path
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    Console.WriteLine("** DEPLOYED");

                    try
                    {
                        String[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(ApplicationDeployment.CurrentDeployment.DataDirectory + "\\" + path));
                        String path3 = System.IO.Path.GetFullPath(path);
                        String file = Array.Find(files, s => (s.ToLower() == path3.ToLower()));
                        Console.WriteLine("** " + files.Count() + " - " + file);
                        if (file != "" && file != null) return file;
                        else return "-1";
                    }
                    catch
                    {
                        Console.WriteLine("File not found:" + path);
                        return "-1";
                    }
                }
                else{
                    Console.WriteLine("** NOT DEPLOYED");*/

                    Console.WriteLine("File not found:" + path);
                    
                    
                    return "-1";

                
            }

        }

        public override void loadConfig()//XElement config)
        {
            //Nothing to load
        }
    }
}
