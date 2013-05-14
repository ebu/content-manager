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
using System.IO;
using ContentManager.GUI;

namespace ContentManager
{
    public class Log
    {
        private static Log mySingleton;
        private TextWriter f;
        private Log()
        {
            logMessage("start Content Manager", "SYS");
        }

        ~Log()
        {
            logMessage("end of program", "SYS");
            
        }

        private Object lck = new Object();
        private void logMessage(String msg, String label = "")
        {
            if (System.Configuration.ConfigurationManager.AppSettings["debug"].ToLower().Equals("true"))
            {
                lock (this.lck)
                {
                    try
                    {
                        this.f = new StreamWriter(getFileName(), true);

                        if (label != "") label = "[" + label + "]";
                        String str = DateTime.Now.ToLocalTime() + "\t" + label + "\t" + msg;
                        Console.WriteLine(str);
                        this.f.WriteLine(str);
                        this.f.Close();

                        UIMain.errorAdd(msg, label);
                    }
                    catch (Exception e)
                    {
                        UIMain.errorAdd("(" + Path.GetFullPath(getFileName()) + "): " + e.Message, "LOGFILE");
                    }
                }
            }
        }
        private static Object singleLck = new Object();
        public static Log getInstance()
        {
            lock (singleLck)
            {
                if (mySingleton == null)
                    mySingleton = new Log();

                return mySingleton;
            }
        }

        public static void log(String msg, String label = "")
        {
            Log.getInstance().logMessage(msg, label);
        }
        public static String getLogFile()
        {
            return Log.getInstance().getFileName();
        }
        public String getFileName()
        {
            return Path.GetFullPath("trace.log");
        }
    }
}
