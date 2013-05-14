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
using System.Windows;
using SlideGeneratorLib.Exceptions;

namespace SlideGeneratorLib.Rendering 
{
    abstract class ARender : IRenderable
    {
        private String tag;

        public ARender(string tag)
        {
            this.tag = tag;
        }
        
        abstract public void draw(XElement e, Canvas c, Dictionary<String, String> dic);

        

        abstract public void draw(XElement e, Canvas c);
        abstract public void loadConfig();//XElement config);

        public String getTag()
        {
            return tag;
        }

        protected double getPositionOrSizeValue(String val, double pixel)
        {
            double ret = 0.0;
            if (val.EndsWith("%"))
            {
                val = val.Substring(0, val.Length - 1);
                ret = Double.Parse(val)/100.0 * pixel;
            }
            else
                ret = Double.Parse(val);
            return ret;
        }

        protected void addToCanvas(XElement e, FrameworkElement uielem, Canvas c)
        {

            /** Size **/
            if (e.Attribute("width") != null)
                uielem.Width = getPositionOrSizeValue(e.Attribute("width").Value.ToString(), c.Width);
            if (e.Attribute("height") != null)
                uielem.Height = getPositionOrSizeValue(e.Attribute("height").Value.ToString(), c.Height);

            /** Opacity **/
            if (e.Attribute("opacity") != null)
                uielem.Opacity = Double.Parse(e.Attribute("opacity").Value.ToString()) / 100.0;
            
            c.Children.Add(uielem);
            
            if (e.Attribute("top") != null)
                Canvas.SetTop(uielem, getPositionOrSizeValue(e.Attribute("top").Value.ToString(), c.Height));
            else if (e.Attribute("bottom") != null)
                Canvas.SetBottom(uielem, getPositionOrSizeValue(e.Attribute("bottom").Value.ToString(), c.Height));

            if (e.Attribute("left") != null)
                Canvas.SetLeft(uielem, getPositionOrSizeValue(e.Attribute("left").Value.ToString(), c.Width));
            else if (e.Attribute("right") != null)
                Canvas.SetRight(uielem, getPositionOrSizeValue(e.Attribute("right").Value.ToString(), c.Width));
            
        }



    }
}
