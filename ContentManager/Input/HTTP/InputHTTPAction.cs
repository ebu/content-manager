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
using ContentManager.GUI;
using System.IO;

namespace ContentManager.Input.HTTP
{
    public class InputHTTPAction
    {

        ContentManagerCore core;
        public InputHTTPAction(ContentManagerCore core)
        {
            this.core = core;
        }

        internal void update(Dictionary<string, string> dictionary)
        {
            core.update(dictionary);
        }

        internal String updateAndBroadcast(Dictionary<string, string> dictionary, String broadcastslidename)
        {
            core.update(dictionary);
            return core.broadcast(broadcastslidename);
        }

        internal String broadcast(String broadcastslide)
        {
            return core.broadcast(broadcastslide);
        }

        internal String change(Dictionary<string, string> dictionary)
        {
            String str = "";
            if(dictionary.ContainsKey("slidedir")){

                try
                {
                    core.slidegen.setSlideDir(dictionary["slidedir"]);
                }
                catch
                {
                    str+= "DIR:"+dictionary["slidedir"]+" not found\n\n";
                }

                refresh();
                core.checkAfterChangeDir();
            }
            if (dictionary.ContainsKey("template"))
            {
                String template = dictionary["template"];
                List<String> l = core.slidegen.getAvailableTemplates();
                if (l.Contains(template))
                    core.slidegen.setCurrentTemplate(template);

                foreach (String s in l)
                    str += s + "\n";
            }
            return str;
        }

        internal String loadslidecart(Dictionary<string, string> dictionary)
        {
            

            List<String> l = new List<String>();
            if (dictionary.Keys.Count != 0)
            {
                foreach(KeyValuePair<String,String> t in dictionary){
                    if (t.Key.StartsWith("SLIDE") && core.slidegen.getAvailableSlides().Contains(t.Value))
                    {
                        l.Add(t.Value);
                        Console.WriteLine("New slide in cart: " + t.Value);
                    }
                    else
                    {

                        return "UNKNOWN ARGUMENT : "+t.Key+" OR UNKNOWN SLIDE:"+t.Value;
                    }
                }
                if (l.Count == 0)
                    return "BAD ARGUMENTS : SLIDE1 SLIDE2 etc..";
            }

            UIMain.Instance.Dispatcher.Invoke((Action)delegate()
            {
                core.engine.setSlideCart(l);
                core.engine.startAutoBroadcast();
            });

            return "OK";
        }

        internal string preview(string slidename, Dictionary<string, string> dic)
        {
            dic.Remove("SLIDE");

            return Path.GetFullPath(core.tmpfolder+core.engine.preview(slidename, dic));                
            
           
        }

        internal string refresh()
        {
            String s = "Available slides: ";
            List<String> slides = core.slidegen.getAvailableSlides();
            s+=slides.Count+"\n";
            foreach (String slide in slides)
                s += slide + "\n";
            return s;
        }

        internal string system(Dictionary<string, string> dictionary)
        {
            if (dictionary.ContainsKey("command"))
            {
                if (dictionary["command"] == "refresh")
                {
                    return refresh();
                }
                else
                    return "Unknown command";
            }
            else
                return "Argument command is missing";

            
        }

        internal String Execute(InputHTTPResult res) 
        {
            String msg = "";
            switch (res.command)
            {
                case InputHTTPResult.INPUTCOMMAND.UPDATE:
                    update(res.parameters);
                    break;
                case InputHTTPResult.INPUTCOMMAND.UPDATEANDBROADCAST:
                    if (res.parameters.ContainsKey("BSLIDE") && res.parameters["BSLIDE"] != "")
                    {
                        if (core.slidegen.getAvailableSlides().Contains(res.parameters["BSLIDE"]))
                            msg = this.updateAndBroadcast(res.parameters, res.parameters["BSLIDE"]);
                        else
                        {
                            msg = "SLIDE NOT FOUND (" + res.parameters["BSLIDE"] + ")";
                            throw new InputHTTPException(msg);
                        }
                    }
                    else
                    {
                        this.update(res.parameters);
                    }
                    break;
                case InputHTTPResult.INPUTCOMMAND.BROADCASTSLIDE:
                    if (res.parameters.ContainsKey("BSLIDE") && res.parameters["BSLIDE"] != "")
                    {
                        if (core.slidegen.getAvailableSlides().Contains(res.parameters["BSLIDE"]))
                            msg = this.broadcast(res.parameters["BSLIDE"]);
                        else
                        {
                            msg = "SLIDE NOT FOUND (" + res.parameters["BSLIDE"] + ")";
                            throw new InputHTTPException(msg);
                        }
                    }
                    else
                    {
                        msg = "Broadcast BAD FORMAT: BSLIDE argument is missing.";
                        throw new InputHTTPException(msg);
                    }
                    break;
                case InputHTTPResult.INPUTCOMMAND.CHANGE:
                    msg = this.change(res.parameters);
                    break;
                case InputHTTPResult.INPUTCOMMAND.LOADSLIDECART:
                    msg = this.loadslidecart(res.parameters);
                    break;
                case InputHTTPResult.INPUTCOMMAND.SYSTEM:
                    msg = this.system(res.parameters);
                    break;
                case InputHTTPResult.INPUTCOMMAND.PREVIEW:
                    if (res.parameters.ContainsKey("SLIDE") && res.parameters["SLIDE"] != "")
                    {
                        if (core.slidegen.getAvailableSlides().Contains(res.parameters["SLIDE"]))
                            msg = this.preview(res.parameters["SLIDE"], res.parameters);
                        else
                        {
                            msg = "ERROR: SLIDE " + res.parameters["SLIDE"] + " is not available";
                            throw new InputHTTPException(msg);
                        }
                    }
                    else
                    {
                        msg = "ERROR: SLIDE argument is missing";

                        throw new InputHTTPException(msg);
                    }
                    break;
            }
            return msg;
        }
    }

    class InputHTTPException : Exception {
        public InputHTTPException(string msg): base(msg) {}
    }
}
