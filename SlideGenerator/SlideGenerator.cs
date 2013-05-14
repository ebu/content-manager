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
using System.Linq;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows;
using SlideGeneratorLib.Exceptions;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Collections.Generic;
using SlideGeneratorLib.Rendering;
using SlideGeneratorLib.Parser;
using System.Threading;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Imaging;
using XMLConfig.CMS;


namespace SlideGeneratorLib
{
    /// <summary>
    /// init Synthax:
    ///        slideGen = new SlideGenerator("D:\\VSPRJ\\EBU\\EBUPrj\\EBUtests\\bin\\Debug\\conf.xml");
    ///        slideGen.newTag("text", new TextRender());
    ///        slideGen.newTag("img", new ImgRender());
    ///        slideGen.newTag("background", new BackgroundRender());
    ///        slideGen.newTag("template", new TemplateRender());
    /// </summary>
    public class SlideGenerator
    {
        Dictionary<String, IRenderable> tags;
        
        int cWidth = CMSConfig.imagewidth;
        int cHeight = CMSConfig.imageheight;
        public static String slidefolder = "";
        Dictionary<String,String> slides;
        public Dictionary<String, String> cstlist;
        public static string tmpfolder="";
        public static string datafolder = "";
        private String urlconfig = "";
        private String urlvariables = "";
        public static String templatefolder = "";

        public static String currentTemplate = "default";
        

        public SlideGenerator(string urlconfig, string urlvariables, string dataflder, string tmpflder)
        {
            this.urlvariables = urlvariables;
            this.urlconfig = urlconfig;
            datafolder = dataflder;
            tmpfolder = System.IO.Path.GetFullPath(tmpflder);
            cstlist = new Dictionary<string, string>();
            tags = new Dictionary<String, IRenderable>();
            slides = new Dictionary<string, string>();
            Console.WriteLine("XML: Loading config file " + urlconfig);

            this.cWidth = CMSConfig.imagewidth;
            this.cHeight = CMSConfig.imageheight;

            loadConfiguration();

            try
            {
                
                init();

            }
            catch (Exception er)
            {

                Console.WriteLine("Unable to open " + urlconfig + " or to locate tmp folder");
                Console.WriteLine("error: " + er.Message);
            }
        }

        private void loadConfiguration()
        {



            loadFolders();

            Console.WriteLine("TMP FOLDER: " + tmpfolder);
            Console.WriteLine("DATA FOLDER: " + datafolder);
            Console.WriteLine("SLIDE FOLDER: " + slidefolder);
            Console.WriteLine("TEMPLATE FOLDER: " + templatefolder);
            if (CMSConfig.ftp.Count == 0)
                MessageBox.Show("No FTP defined. The content manager is offline. So, the ONAIR frame will not be able to display broadcasted slides. However you can use right click to preview the slides", "Warning");
            


            loadVariables();
            loadSlides();
            
        }

        private void init()
        {
            
            Console.WriteLine("Loading text tag");
            this.newTag("text", new TextRender(this));
            Console.WriteLine("Loading img tag");
            this.newTag("img", new ImgRender(this.cstlist));
            Console.WriteLine("Loading imgrdm tag");
            this.newTag("imgrdm", new ImgRandomRender(this.cstlist));
            Console.WriteLine("Loading background tag");
            this.newTag("background", new BackgroundRender());
            Console.WriteLine("Loading template tag");
            this.newTag("template", new TemplateRender());
        }

        private void loadVariables()
        {
            try
            {
                XElement e = XElement.Load(this.urlvariables);
                VarParser.XMLrequire(e, "variables");

                List<XElement> vars = e.Elements().ToList();
                Console.WriteLine(vars.Count + " elements");
                for (int i = 0; i < vars.Count; i++)
                {
                    String key = vars.ElementAt(i).Name.ToString().ToUpper();
                    String val = vars.ElementAt(i).Value;
                    Console.WriteLine(key + " added to cstList with val=" + val);
                    cstlist.Add(key, val);
                }
            }
            catch (XMLNotRecognizedElement er)
            {
                Console.WriteLine("XMLNotRecognizedElement :" + er.Message);
                Console.WriteLine("FATAL ERROR: exit(1)");
                Environment.Exit(1);
            }
        }

        private void loadSlides()
        {
            DirectoryInfo i = new DirectoryInfo(slidefolder);
            Console.WriteLine("Available slides:");
            slides.Clear();
            foreach(FileInfo f in i.EnumerateFiles("*.xml"))
            {
                String slidename = f.Name.Substring(0, f.Name.Length - 4);
                String err = "";
                if (slidename.Contains("."))
                    err = "=>ERROR FILENAME CONTAINS A POINT -> not loaded";
                else if(!slides.Keys.Contains(slidename))
                    slides.Add(slidename, f.FullName);
                Console.WriteLine(slidename+err);
                
            }
        }


        private void loadFolders(String slidef="")
        {
           

            String sf = slidef;
            if(sf=="") sf=CMSConfig.dirslide;
            
            if (!Directory.Exists(sf) && ApplicationDeployment.IsNetworkDeployed)
                sf = ApplicationDeployment.CurrentDeployment.DataDirectory + "\\" + sf;
            if (Directory.Exists(sf))
                slidefolder = sf;
            else
            {
                
                throw new DirectoryNotFoundException("Unable to find " + sf + " directory");
            }
            Console.WriteLine("NEW SlideFolder: " + slidefolder);

            String tf = CMSConfig.dirtemplate;
            if (!Directory.Exists(tf) && ApplicationDeployment.IsNetworkDeployed)
                tf = ApplicationDeployment.CurrentDeployment.DataDirectory + "\\" + tf;
            if (Directory.Exists(tf))
                templatefolder = tf;
 
            else
            {
                
                throw new DirectoryNotFoundException("Unable to find " + tf + " directory");
            }
            Console.WriteLine("NEW templatefolder: " + templatefolder);
        }

        private void newTag(String tag, IRenderable renderengine)
        {
            tags.Add(tag, renderengine);
            renderengine.loadConfig();//config);
        }

        /// <summary>
        /// Generate slide "name" and return a canvas containing the graphical slide
        /// </summary>
        /// <param name="name">slide key for slides Dictionary (if name contains a "." interpreted as an url)</param>
        /// <returns></returns>
        public SlideResult loadXMLSlide(String name, Dictionary<String, String> dic=null)
        {

            DateTime n = DateTime.Now;
            Canvas c = new Canvas();
            String url="";
            String textresult = "Text Result ...";
            String link = "";
            int broadcastdelay = CMSConfig.broadcastdelay;

            Console.WriteLine("search for :" + name);
            try
            {
                if (name.Contains("."))
                    url = name;
                else
                    url = slides[name];
            }
            
            catch { return null;  }
            Console.WriteLine("search ok!");
            c.Width = this.cWidth;
            c.Height = this.cHeight;
            c.Background = System.Windows.Media.Brushes.LightGray;
            Console.WriteLine("url:"+url);
            XElement e = null;
            try
            {
                e = XElement.Load(url);
            }
            catch (Exception er)
            {
                Console.WriteLine("=> Unable to open " + url + "");
                return null;
            }

            VarParser.XMLrequire(e, "sequence");

            List<XElement> slideList = e.Elements().ToList();

            for (int i = 0; i < slideList.Count; i++)
            {

                XAttribute a = slideList.ElementAt(i).Attribute("link");
                if (a != null)
                    link = a.Value;


                XAttribute b = slideList.ElementAt(i).Attribute("broadcastdelay");
                if (b != null)
                    broadcastdelay = Int32.Parse(b.Value);
                
                List<XElement> elementList = slideList.ElementAt(i).Elements().ToList();
                Console.WriteLine("slide " + i + " has " + elementList.Count + " elements");
                for (int j = 0; j < elementList.Count; j++)
                {
                    try
                    {
                        XElement elem = elementList.ElementAt(j);
                        Console.WriteLine("rendering element " + j + " of slide " + i + " " + elem.Name.ToString());
                        drawXElement(elem, c, dic);
                    }
                    catch (XMLNotRecognizedElement er)
                    {
                        Console.WriteLine("XMLNotRecognizedElement : " + er.Message);
                    }
                    elementList = slideList.ElementAt(i).Elements().ToList();
                }

            }
            
            RectangleGeometry r = new RectangleGeometry(new Rect(0, 0, CMSConfig.imagewidth, CMSConfig.imageheight));
            c.Clip = r;
            Console.WriteLine("Slide " + name + " generated in "+DateTime.Now.Subtract(n).TotalMilliseconds+"ms");
            VarParser.clearRSSCache();
            return new SlideResult(name, url, c, textresult, link, broadcastdelay);
        }

        private void drawXElement(XElement e, Canvas c, Dictionary<String,String> dic=null)
        {

            String tag = e.Name.ToString();
            try
            {
                IRenderable engine = tags[tag];
                if(dic!=null)
                    engine.draw(e, c, dic);
                else
                    engine.draw(e, c);

            }
            catch (KeyNotFoundException err)
            {
                throw new XMLNotRecognizedElement("Engine for tag:{" + tag + "} not found");
            }

        }



        /// <summary>
        /// Save an UIElement to a png file
        /// </summary>
        /// <param name="source">Element to save</param>
        /// <param name="filename">png filename</param>
        public void saveToPng(UIElement source, String filename)
        {
            source.Measure(new System.Windows.Size(CMSConfig.imagewidth, CMSConfig.imageheight));
            source.Arrange(new System.Windows.Rect(0.0, 0.0, CMSConfig.imagewidth, CMSConfig.imageheight));
            RenderTargetBitmap rtbImage = new RenderTargetBitmap(CMSConfig.imagewidth,
               CMSConfig.imageheight,
               96,
               96,
               PixelFormats.Pbgra32);
            rtbImage.Render(source);

            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtbImage));

            using (Stream stm = File.Create(filename))
            {
                png.Save(stm);
            }

        }

        public void saveToJpg(UIElement source, String filename, String link="", int quality=100)
        {
            saveToJpg(source, filename, link, true, quality);
            /*
            source.Measure(new System.Windows.Size(320.0, 240.0));
            source.Arrange(new System.Windows.Rect(0.0, 0.0, 320.0, 240.0));
            RenderTargetBitmap rtbImage = new RenderTargetBitmap(320,
               240,
               96,
               96,
               PixelFormats.Pbgra32);
            rtbImage.Render(source);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            

            BitmapFrame outputFrame = BitmapFrame.Create(rtbImage);
            encoder.Frames.Add(outputFrame);
            encoder.QualityLevel = 100;
            try
            {
                using (FileStream file = File.OpenWrite(filename))
                {
                    encoder.Save(file);
                }
            }
            catch
            {
                Console.WriteLine("Error writting files");
            }*/
        }
        private void saveJpeg(string path, Bitmap img, long quality)
        {
            // Encoder parameter for image quality
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

            // Jpeg image codec
            ImageCodecInfo jpegCodec = getEncoderInfo("image/jpeg");

            if (jpegCodec == null)
                return;

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(path, jpegCodec, encoderParams);
        }

        private static ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        public static void saveToJpg(UIElement source, string filename, String link, bool include_exif, int quality=100)
        {
            RenderOptions.SetEdgeMode(source, EdgeMode.Aliased);
            RenderOptions.SetBitmapScalingMode(source, BitmapScalingMode.HighQuality);
            source.SnapsToDevicePixels = true;
            source.Measure(source.RenderSize);//new System.Windows.Size(source.RenderSize.Width, source.RenderSize.Height));
            source.Arrange(new Rect(source.RenderSize));//new System.Windows.Rect(0.0, 0.0, 320.0, 240.0));
            //RenderOptions.SetBitmapScalingMode(source, BitmapScalingMode.Linear);
            
            RenderTargetBitmap rtbImage = new RenderTargetBitmap(CMSConfig.imagewidth,
               CMSConfig.imageheight,
               96,
               96,
               PixelFormats.Default);
            
            rtbImage.Render(source);
            
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();

         /*   String v = "http://radiodns1.ebu.ch/";
            BitmapMetadata m = new BitmapMetadata("jpg");
            //m.Comment = v; 37510
            m.SetQuery("/app1/ifd/exif:{uint=270}", v);*/
            //BitmapFrame outputFrame = BitmapFrame.Create(rtbImage, rtbImage, m, null);

            int width = rtbImage.PixelWidth;
            int height = rtbImage.PixelHeight;
            int stride = width * ((rtbImage.Format.BitsPerPixel + 7) / 8);

            byte[] bits = new byte[height * stride];

            MemoryStream mem = new MemoryStream();
            BitmapEncoder bitmapenc = new BmpBitmapEncoder();
            bitmapenc.Frames.Add(BitmapFrame.Create(rtbImage));

            bitmapenc.Save(mem);


            Bitmap bmp1 = new Bitmap(mem);
            
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Compression;
            System.Drawing.Imaging.Encoder myEncoder2 =
                System.Drawing.Imaging.Encoder.Quality;

            
            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(2);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 0L);
            EncoderParameter myEncoderParameter2 = new EncoderParameter(myEncoder2, quality);
            myEncoderParameters.Param[0] = myEncoderParameter;
            myEncoderParameters.Param[1] = myEncoderParameter2;
            bmp1.Save(filename, jgpEncoder, myEncoderParameters);

            /*
            BitmapFrame outputFrame = BitmapFrame.Create(rtbImage);
            
            encoder.QualityLevel = quality;
            encoder.Frames.Add(outputFrame);
            */
            /*
            try
            {
                using (FileStream file = File.OpenWrite(filename))
                {
                    encoder.Save(file);
                    file.Close();
                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error writting files\n"+e.Message);
            }*/
        }
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public List<string> getAvailableSlides()
        {
            this.loadSlides();
            return this.slides.Keys.ToList();
        }

        public List<String> getAvailableTemplates()
        {
            List<String> ret = new List<string>();
            DirectoryInfo i = new DirectoryInfo(SlideGenerator.templatefolder);
            Console.WriteLine("Available templates:");
            
            foreach (FileInfo f in i.EnumerateFiles("*.xml"))
            {
                String templatename = f.Name.Substring(0, f.Name.Length - 4);
                ret.Add(templatename);
                Console.WriteLine("->"+templatename);
            }

            return ret;
        }
        public List<string> getAvailableSlides(String prefix)
        {
            this.loadSlides();
            return this.slides.Keys.TakeWhile(key => key.IndexOf(prefix) == 0).ToList();
        }

        public void setVar(string key, string content)
        {
            key = key.ToUpper();
            try
            {
                Console.WriteLine("SET " + key + " = " + content);
                if (!cstlist.ContainsKey(key))
                    cstlist.Add(key, content);
                else
                    cstlist[key] = content;
            }
            catch { Console.WriteLine(key + " not found"); }
        }
       
        public string getVar(string key)
        {
            key = key.ToUpper();
            try
            {
                return cstlist[key];
            }
            catch { Console.WriteLine(key +" not found"); }
            return "-- error --";
        }

        public void setCurrentTemplate(string p)
        {
            currentTemplate = p;
            this.tags["template"].loadConfig();
        }


        public string getCurrentTemplate()
        {
            return currentTemplate;
        }

        public void setSlideDir(string newdir)
        {
            String sf = newdir;
            if (!sf.Contains("\\") && !sf.Contains("/"))
                sf = CMSConfig.dirslide +"/"+ sf;

            this.loadFolders(sf);
        }

        public List<String> getSlideVariables(string slideName)
        {
            List<String> list = new List<string>();
            
            TextReader tr = new StreamReader(slides[slideName]);
            String f = tr.ReadToEnd();
            
            tr.Close();
            int currentpos=0;

            while (currentpos != -1)
            {
                int start = f.IndexOf("@@@", currentpos);
                if (start != -1)
                {


                    int end = f.IndexOf("@@@", start + 1);
                    String varname = f.Substring(start + 3, end - start - 3);
                    if (!list.Contains(varname)) 
                        list.Add(varname);
                    end = end + 3;
                    currentpos = end;
                }
                else
                    currentpos = -1;

                
            }

            return list;
        }
    }
}


