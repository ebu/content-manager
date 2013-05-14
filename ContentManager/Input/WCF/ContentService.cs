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
using ContentServiceLibrary;
using System.Windows.Controls;
using System.ServiceModel;
using ContentManager.GUI;
using SlideGeneratorLib;

namespace ContentManager
{
    
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ContentService : IContentService
    {
        public IContentServiceCallback callback = null;
        public IContextChannel channel = null;

        public ContentService()
        {
            this.callback = OperationContext.Current.GetCallbackChannel<IContentServiceCallback>();
            UIMain.core.engine.onBroadcast += new BroadcastEngine.OutputEvent(autobroadcast_onBroadcast);
            channel = OperationContext.Current.Channel;
        }

        void autobroadcast_onBroadcast(String slidename, String url)
        {
         //   Console.WriteLine("[CONTENTSERVICE] ON BROADCAST > CALLBACK :"+DateTime.Now.ToLongTimeString()+" "+b.slide);
            try
            {
                if (channel.State == CommunicationState.Opened)
                    callback.OnOnAirChange(slidename);

            }
            catch
            {
                UIMain.errorAdd("Callback aborted"+ channel.State);

            }
        }
            

        public List<string> getAvailableSlides()
        {
            return UIMain.core.slidegen.getAvailableSlides();
        }

        public List<string> getAvailableSlides(string prefix)
        {

            return UIMain.core.slidegen.getAvailableSlides(prefix);
        }

        public String broadcast(string slidekey)
        {
            Console.WriteLine(" BROADCAST " + slidekey + "!");
            UIMain.core.broadcast(slidekey);
            return "";
        }

        public String getPreview(string slidekey)
        {

            Console.WriteLine(" PREVIEW " + slidekey + "!");
            //MainWindow.slidegen.
            SlideResult r =  UIMain.core.slidegen.loadXMLSlide(slidekey);
            Canvas c = r.image;
            if (c == null) return null;
            String url=""+slidekey+"-"+DateTime.Now.ToFileTime()+".jpg";
            UIMain.core.slidegen.saveToJpg(c, SlideGenerator.tmpfolder+url);
            return url;
        }


        public void setVar(string key, string content)
        {
            UIMain.core.slidegen.setVar(key, content);
        }

        public string getVar(string key)
        {
            return UIMain.core.slidegen.getVar(key);
        }


        public void setAutoBroadcast(bool isEnabled)
        {
            if(!isEnabled)
                UIMain.core.engine.stopAutoBroadcast();
            else
                UIMain.core.engine.startAutoBroadcast();
            
        }

        public bool getAutoBroadCastEnabled()
        {
            return UIMain.core.engine.isAutoBroadcastEnabled();
        }

        public void setAutoBroadcastParameters(TimeSpan Interval, List<string> slidekeys)
        {
            UIMain.core.engine.setSlideCart(slidekeys);
            UIMain.core.engine.setInterval(Interval);
            
        }

        public List<string> getAutoBroadcastSlides()
        {
            return UIMain.core.engine.slidecart;
        }

        public TimeSpan getAutoBroadcastInterval()
        {
            return UIMain.core.engine.getInterval();
        }
        public String getTmpFolder()
        {
            return SlideGenerator.tmpfolder;
        }

    }
}
