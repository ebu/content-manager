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
using System.Windows.Controls;
using System.Xml.Linq;
using SlideGeneratorLib.Exceptions;
using SlideGeneratorLib.Parser;
using System.Windows.Media;
using System.Windows;
using System.Windows.Documents;

namespace SlideGeneratorLib.Rendering
{
    class TextRender: ARender
    {
        private SlideGenerator slidegen;
        public TextRender(SlideGenerator slidegen) : base("text") { this.slidegen = slidegen; }

        public override void draw(XElement field, Canvas c)
        {
            draw(field, c, null);    
        }
        public override void draw(XElement field, Canvas c, Dictionary<string, string> localdic)
        {
            if (field.HasAttributes && field.Attribute("content") != null)
            {
                TextBlock box = new TextBlock();
                box.TextWrapping = TextWrapping.WrapWithOverflow;
                /** Text **/
                String t = VarParser.parseText(field.Attribute("content").Value, this.slidegen.cstlist, localdic);
                t = t.Replace("\\n", "\n");
                box.Inlines.Add(t);

                /** Font **/
                if (field.Attribute("font-style") != null)
                {
                    String dec = field.Attribute("font-style").Value;
                    if (dec.IndexOf("bold") != -1)
                    {
                        box.FontWeight = FontWeights.Bold;
                    }
                    if (dec.IndexOf("italic") != -1)
                    {
                        box.FontStyle = FontStyles.Italic;
                    }
                    if (dec.IndexOf("underline") != -1)
                    {
                        box.TextDecorations = TextDecorations.Underline;
                    }
                    
                }
                if (field.Attribute("font-size") != null)
                {
                    box.FontSize = Int32.Parse(field.Attribute("font-size").Value);
                }
                if (field.Attribute("font-color") != null)
                {
                    box.Foreground = new SolidColorBrush(ColorParser.parse(field.Attribute("font-color").Value.ToString()));
                }
                if (field.Attribute("font-family") != null)
                {
                    box.FontFamily = new FontFamily(field.Attribute("font-family").Value);                 
                }

                /** Text Alignement **/
                if (field.Attribute("text-align") != null)
                {
                    switch(field.Attribute("text-align").Value.ToString()){
                        case "left":
                            box.TextAlignment = System.Windows.TextAlignment.Left;
                            break;
                        case "right":
                            box.TextAlignment = System.Windows.TextAlignment.Right;
                            break;
                        case "center":
                            box.TextAlignment = System.Windows.TextAlignment.Center;
                            break;
                    }
                }
                

                addToCanvas(field, box, c);
            }
        }


        public override void loadConfig()//XElement config)
        {


        }



    }
}
