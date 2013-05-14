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
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using SlideGeneratorLib.Parser;
using XMLConfig.CMS;

namespace SlideGeneratorLib.Rendering
{
    class BackgroundRender:ARender
    {
        public BackgroundRender() : base("background") { }

        Brush bgcolor = Brushes.Transparent;
        int cWidth = CMSConfig.imagewidth;
        int cHeight = CMSConfig.imageheight;
        public override void draw(System.Xml.Linq.XElement e, System.Windows.Controls.Canvas c)
        {
            if (e.Attribute("color") != null)
            {
                String col = e.Attribute("color").Value;
                Console.WriteLine("BGCOL : " + col);
                Rectangle rect = new Rectangle();
                rect.Height = cHeight;//HARD CODED BUG
                rect.Width = cWidth;
                rect.Fill = new SolidColorBrush(ColorParser.parse(col));
                addToCanvas(e, rect, c);
            }
        }

        public override void draw(XElement e, System.Windows.Controls.Canvas c, Dictionary<string, string> dic)
        {
            draw(e, c);
        }

        public override void loadConfig()//System.Xml.Linq.XElement config)
        {
           /* IEnumerable<XElement> elements =
            from el in config.Elements()
            where el.Name == "render"
            select el;
           
            XElement erender = elements.First();
            VarParser.XMLrequire(erender, "render");

            if (erender.Attribute("width") != null)
                this.cWidth = (int)erender.Attribute("width");
            if (erender.Attribute("height") != null)
                this.cHeight = (int)erender.Attribute("height");*/
        }
    }
}
