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
using SlideGeneratorLib.Exceptions;
using System.Xml;

namespace SlideGeneratorLib.Parser
{
    public class VarParser
    {
        
        private static Dictionary<String, String> rsscache = new Dictionary<string, string>();

        public static String parseText(String str, Dictionary<String, String> dic, Dictionary<String, String> primarydic)
        {
            if (primarydic != null)
            {
                Console.WriteLine("\n\n####");
                foreach (KeyValuePair<String, String> p in primarydic)
                {

                    Console.WriteLine(p.Key + " - " + p.Value);
                }
            }

            while (str.IndexOf("@@@") != -1)
            {

                int pstart = str.IndexOf("@@@") + 3;
                int pend = str.IndexOf("@@@", pstart);
                if (pend == -1)
                {
                    throw new XMLSynthaxErrorException("@@@{varname}@@@ tag error. Check text element {content='" + str + "'}");
                }
                String var = str.Substring(pstart, pend - pstart);
               // Console.WriteLine("VAR: " + var);

                String val = "--Error--";
                if (var == "DATE")
                {
                    val = DateTime.Now.ToLongDateString();
                }
                else if (var == "SHORTDATE")
                {
                    val = DateTime.Now.ToShortDateString();
                }
                else if (var == "TIME")
                {
                    val = DateTime.Now.ToLongTimeString();
                }
                else if (var =="SHORTTIME")
                {
                    val = DateTime.Now.ToShortTimeString();
                }
                else
                {

                    if (primarydic != null && primarydic.ContainsKey(var))
                    {
                        Console.WriteLine("\n\n****************\n" + var);
                        val = primarydic[var];
                    }
                    else
                    {
                        try
                        {
                            val = dic[var.ToUpper()];

                            if (val.StartsWith("rss@"))
                                val = parseRSS(val);

                            // Console.WriteLine("VAL: " + val);
                        }
                        catch (KeyNotFoundException e)
                        {
                            Console.WriteLine("Key " + var + " not found: " + e.Message);
                        }
                    }
                }
                str = str.Replace("@@@" + var + "@@@", val);
            }


            return str;
        }

        public static List<String> getProtectedVariables()
        {
            List<String> l = new List<String>();
            l.Add("SHORTTIME");
            l.Add("SHORTDATE");
            l.Add("TIME");
            l.Add("DATE");
            return l;
        }


        public static void clearRSSCache()
        {
            rsscache.Clear();
        }

        public static string parseRSS(string val, XmlDocument doc=null)
        {

            Console.WriteLine("RSS **************************");
            String url = "";
            String scheme = "";
            String [] s = val.Split(("@").ToArray(),StringSplitOptions.None);
            if (s.Length == 3)
            {
                url = s[1];
                scheme = s[2];

                try
                {
                    Console.WriteLine("RSS **************************");
                    if(doc!=null){
                        String u = SlideGeneratorLib.SlideGenerator.tmpfolder + "xmltmp-" + DateTime.Now.ToFileTime()+".xml";
                        Console.WriteLine(u);
                        doc.Save(u);
                        url = u;
                    }


                    XmlReader xml = XmlReader.Create(url);

                    //rsscache.xml.ReadSubtree();

                    /* xml.ReadToDescendant("channel");
                     xml.ReadToDescendant("item");
                     xml.ReadToFollowing("item");
                     xml.ReadToDescendant("title");
                     String k = xml.ReadElementContentAsString();

                     Console.WriteLine(xml.Name+" " +k);
                     */
                    String[] nodes = scheme.Split((">").ToArray(), StringSplitOptions.None);

                    String path = "";
                    String attribute = "";
                    for (int i = 0; i < nodes.Length; i++)
                    {


                        String node = nodes.ElementAt(i);
                        int n = getNb(node);
                        if (node.Contains("("))
                            node = node.Substring(0, node.IndexOf("("));

                        if (node.Contains("!"))
                        {
                            String[] l = node.Split(("!").ToArray());
                            node = l[0];
                            attribute = l[1];
                        }
                        path += ":" + node;


                        for (int j = 0; j < n; j++)
                        {
                            if (j == 0)
                            {
                                if (!xml.ReadToDescendant(node))
                                    throw new RssParseException("NOT FOUND TAG '" + node + "' in path" + path);
                            }
                            else
                            {
                                if (!xml.ReadToFollowing(node))
                                    throw new RssParseException("NOT FOUND " + (j + 1) + "th '" + node + "' in path : " + path);
                            }

                        }

                        if (attribute != "")
                            return xml.GetAttribute(attribute);

                    }
                    String str = xml.ReadString();
                    xml.Close();
                    return str;
                }
                catch (Exception e)
                {
                    Console.WriteLine("RSS Exception: " + e.Message);
                    return "--error--";
                }
                
            }


            return "";
        }

        public static string parseCSV(string val, XmlDocument doc = null)
        {

            String[] s = val.Split(("@").ToArray(), StringSplitOptions.None);
            if (s.Length == 2)
            {
                String path = s[1];
                
                try
                {
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("CSV Exception: " + e.Message);
                    return "--error--";
                }

            }


            return "";
        }


        private static int getNb(string node)
        {
            
            int d = 1;
            try{
                if (node.Contains("(") && node.Contains(")"))
                {
                    int start = node.IndexOf("(")+1;
                    int end = node.IndexOf(")");
                    d = Int32.Parse(node.Substring(start, end - start));

                }
            }
            catch(Exception e){
                Console.WriteLine("RssException: "+e.Message);
            }


            return d;
        }

        internal static void XMLrequire(XElement xe, string p)
        {
            if (xe == null)
            {
                throw new XMLNotRecognizedElement("XML ERROR required:" + p + " found:xe == null");
            }
            if (xe.Name == null)
            {
                throw new XMLNotRecognizedElement("XML ERROR required:" + p + " required found:xe.Name == null");

            }
            if (xe.Name != p)
            {
                throw new XMLNotRecognizedElement("XML ERROR required:" + p + " found:" + xe.Name.ToString() + "");

            }
        }
    }
}
