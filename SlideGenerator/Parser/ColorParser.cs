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

namespace SlideGeneratorLib.Parser
{
    class ColorParser
    {
        public static Color parse(String col)
        {
            byte a = 255;
            byte r = (byte)(Convert.ToUInt32(col.Substring(1, 2), 16));
            byte g = (byte)(Convert.ToUInt32(col.Substring(3, 2), 16));
            byte b = (byte)(Convert.ToUInt32(col.Substring(5, 2), 16));
            return Color.FromArgb(a, r, g, b);
        }
    }
}
