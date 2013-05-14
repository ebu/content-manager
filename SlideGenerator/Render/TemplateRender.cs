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
using SlideGeneratorLib.Parser;
using System.Xml.Linq;
using SlideGeneratorLib.Exceptions;
using System.IO;
using System.Deployment.Application;

namespace SlideGeneratorLib.Rendering
{
    class TemplateRender:ARender
    {
        private Dictionary<String, LinkedList<XElement>> templates = new Dictionary<string, LinkedList<XElement>>();
        public TemplateRender()
            : base("template")
        {
        }
        public override void draw(XElement e, System.Windows.Controls.Canvas c, Dictionary<string, string> dic)
        {
            draw(e, c);
        }
        public override void draw(System.Xml.Linq.XElement e, System.Windows.Controls.Canvas c)
        {
            String id = e.Attribute("id").Value;
            try{
                LinkedList<XElement> list = templates[id];
                foreach(XElement l in list){
                    Console.WriteLine(l);
                    e.AddAfterSelf(l);
                }
            }
            catch (KeyNotFoundException err)
            {
                throw new XMLNotRecognizedElement("template "+id+" not found");
            }
        }

        public void reloadTemplates()
        {

        }

        public override void loadConfig()//XElement config)
        {
   

            
            Console.WriteLine("****************\nParse config for Templates");
            templates.Clear();
            try
            {

                
                    String v = SlideGenerator.templatefolder+"\\"+SlideGenerator.currentTemplate+".xml";

                    if (!Directory.Exists(v) && !File.Exists(v) && ApplicationDeployment.IsNetworkDeployed)
                        v = ApplicationDeployment.CurrentDeployment.DataDirectory + "\\" + v;

                    
                    XElement t = XElement.Load(v);
                    
                    VarParser.XMLrequire(t, "templates");
                    IEnumerable<XElement> tmpl =
                    from el in t.Elements()
                    where el.Name == "template"
                    select el;

                    Console.WriteLine("first:"+tmpl.Count());
                    for (int j = 0; j < tmpl.Count(); j++)
                    {
                        String key = tmpl.ElementAt(j).Attribute("id").Value;
                        Console.WriteLine("NEW TEMPLATE " + key + "");
                        LinkedList<XElement> l = new LinkedList<XElement>();
                        foreach (XElement h in tmpl.ElementAt(j).Elements())
                        {
                            l.AddFirst(h);
                        }
                        templates.Add(key, l);
                    }

                

            }
            catch (Exception er)
            {
                Console.WriteLine("XMLNotRecognizedElement :" + er.Message);
                Console.WriteLine("Warning : " + er.Message);
                
            }
            
        }
    }
}
