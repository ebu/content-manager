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

namespace SlideGeneratorLib
{
    public class SlideResult
    {
        public SlideResult(String name, String xmlpath, Canvas image, String text, String link, int broadcastdelay)
        {
            this.name = name;
            this.xmlpath = xmlpath;
            this.image = image;
            this.text = text;
            this.link = link;
            this.broadcastdelay = broadcastdelay;
        }

        public string name { get; set; }

        public string xmlpath { get; set; }

        public Canvas image { get; set; }

        public string text { get; set; }
        
        public string link { get; set; }

        public int broadcastdelay { get; set; }
    }
}
