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

namespace ContentManager.Output.Ftp
{
    public class FtpParam
    {
        public string address;
        public string user;
        public string password;
        public string filename;
        public int minperiod; //seconds
        public int id;
        public string link;
        public Boolean externalprocess;

        public FtpParam(string address, string user, string password, string filename, string link, int minperiod, int id = -1, Boolean externalprocess=false)
        {
            this.address = address;
            this.user = user;
            this.password = password;
            this.filename = filename;
            this.link = link;
            this.externalprocess = externalprocess;
            try
            {
                this.minperiod =minperiod;
            }
            catch
            {
                this.minperiod = 0;
            }
            this.id = id;
        }

    }
}
