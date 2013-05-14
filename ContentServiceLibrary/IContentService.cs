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
using System.ServiceModel;
using System.Windows.Controls;

namespace ContentServiceLibrary
{

    [ServiceContract(CallbackContract = typeof(IContentServiceCallback))]
    public interface IContentService
    {
        /** BROADCAST / PREVIEW ACTIONS **/

        [OperationContract]
        String broadcast(String slidekey);

        [OperationContract]
        String getPreview(String slidekey);

        /** AVAILABLE SLIDES **/
        [OperationContract(Name="getAvailableSlides1")]
        List<String> getAvailableSlides();

        [OperationContract(Name="getAvailableSlides2")]
        List<String> getAvailableSlides(String prefix);
        
        /** AUTO BROADCAST ENABLED **/
        [OperationContract]
        void setAutoBroadcast(Boolean isEnabled);

        [OperationContract]
        Boolean getAutoBroadCastEnabled();

        /** AUTO BROADCAST OPTIONS **/
        [OperationContract]
        void setAutoBroadcastParameters(TimeSpan Interval, List<String> slidekeys);

        [OperationContract]
        List<String> getAutoBroadcastSlides();

        [OperationContract]
        TimeSpan getAutoBroadcastInterval();

        /** DYNAMIC VARS **/
        [OperationContract]
        void setVar(String key, String content);

        [OperationContract]
        String getVar(String key);
        
        /** STATIC VARS **/
        [OperationContract]
        String getTmpFolder();


    }
}
