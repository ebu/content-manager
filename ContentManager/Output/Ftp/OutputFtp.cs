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
using System.Threading;
using ContentManager.GUI;
using System.Net;
using System.IO;
using System.Windows;
using XMLConfig.CMS;
using System.Drawing;
using System.Drawing.Imaging;

namespace ContentManager.Output.Ftp
{
    class OutputFtp
    {
        public delegate void OutputEvent(String filename, String link);
        public event OutputEvent onUploadEnd;

        Dictionary<int, DateTime> lastSend = new Dictionary<int, DateTime>();

        public void send(String localFilename, String tmpPath, string link)
        {


            Thread newThread = new Thread(new ParameterizedThreadStart(this.ThreadProcess));
            LinkedList<FtpParam> ftplist = new LinkedList<FtpParam>();

            int i=0;
            foreach (FtpAccount f in CMSConfig.ftp)
            {
                String ftpserver = f.server;
                String ftpuser = f.login;
                String ftppwd = f.password;
                int ftpminperiod = f.minperiod;
                Boolean externalprocess = f.externalProcess;


                ftplist.AddLast(new FtpParam(ftpserver + "" + localFilename, ftpuser, ftppwd, tmpPath + localFilename, link, ftpminperiod, i, externalprocess));
                Console.WriteLine("NEW FTP");
                i++;
            }
            newThread.Start(ftplist);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private String reprocessImage(String file)
        {

            String filename2 = file+"-low.jpg";
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Compression;
            System.Drawing.Imaging.Encoder myEncoder2 =
                System.Drawing.Imaging.Encoder.Quality;


            // Create an EncoderParameters object.
            // An EncoderParameters object has an array of EncoderParameter
            // objects. In this case, there is only one
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(2);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 0L);
            EncoderParameter myEncoderParameter2 = new EncoderParameter(myEncoder2, CMSConfig.ep_quality);
            myEncoderParameters.Param[0] = myEncoderParameter;
            myEncoderParameters.Param[1] = myEncoderParameter2;


            using (Bitmap bmp1 = new Bitmap(file))
            {

                bmp1.Save(filename2, jgpEncoder, myEncoderParameters);
            }
            return filename2;
        }

        private void sendToFtp(String address, String user, String pwd, String filename, String link, Boolean externalprocess)
        {
            try
            {
                if(externalprocess)
                {
                    UIMain.errorAdd("Ext process");
                    filename = reprocessImage(filename);
                    UIMain.errorAdd("Ext processed");
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.EnableRaisingEvents=false;
                    proc.StartInfo.FileName="D:\\exiftool.exe";
                    proc.StartInfo.Arguments="-all= -comment=\""+link+"\" "+filename;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    proc.WaitForExit();
                }


                Uri uri = new Uri(address.Substring(0, address.Length - 2));

                UIMain.errorAdd("FTP UPLOAD TO: " + uri.AbsoluteUri);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                //RTBF US quick fix 
                request.Proxy = null;
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(user, pwd);

                // Copy the contents of the file to the request stream.
                FileStream stream = new FileStream(filename, FileMode.Open);
                BinaryReader reader = new BinaryReader(stream);
                //StreamReader sourceStream = new StreamReader(filename);
                byte[] fileContents = reader.ReadBytes((int)stream.Length);

                stream.Close();
                reader.Close();
                request.ContentLength = fileContents.Length;
                try
                {
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(fileContents, 0, fileContents.Length);
                    requestStream.Close();
                }
                catch (WebException e)
                {
                    Console.WriteLine("error " + e.Message);
                    UIMain.errorAdd("Error when writting picture on the ftp server: " + e.Message);
                }
                FtpWebResponse response = null;
                try
                {
                    response = (FtpWebResponse)request.GetResponse();
                    request = (FtpWebRequest)WebRequest.Create(uri);
                    request.Proxy = null;
                    request.Credentials = new NetworkCredential(user, pwd);
                    request.Method = WebRequestMethods.Ftp.Rename;

                    // MainWindow.errorAdd("filename:"+filename.Substring(filename.LastIndexOf(@"\")+1));
                    request.RenameTo = filename.Substring(filename.LastIndexOf(@"\") + 1);
                    UIMain.errorAdd("FTP RENAME to " + request.RenameTo);
                    //if (alone == "yes") request.RenameTo = "ONAIR.jpg";
                    response = (FtpWebResponse)request.GetResponse();


                }
                catch (WebException e)
                {
                    Console.WriteLine("error " + e.Message);
                    UIMain.errorAdd("Error when sending picture on the ftp server: " + e.Message);
                }

                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                UIMain.errorAdd("FTP OK");
                response.Close();

                // HACK : if external process don't send to stomp.
                if (!externalprocess)
                 this.onUploadEnd(filename.Substring(filename.LastIndexOf(@"\") + 1), link);
                

            }
            catch (Exception e)
            {
                UIMain.errorAdd("Error when sending picture on the ftp server" + e.Message);
            }
        }

        private void ThreadProcess(object s)
        {
            LinkedList<FtpParam> ftplist = (LinkedList<FtpParam>)s;
            for (int i = 0; i < ftplist.Count; i++)
            {
                FtpParam ftp = ftplist.ElementAt(i);
                if (!this.lastSend.ContainsKey(ftp.id))
                {
                    this.lastSend.Add(ftp.id, DateTime.Now);
                    sendToFtp(ftp.address, ftp.user, ftp.password, ftp.filename, ftp.link, ftp.externalprocess);
                }
                else
                {
                    lock (this.lastSend)
                    {
                        if (this.lastSend[ftp.id].AddSeconds(ftp.minperiod) < DateTime.Now)
                        {
                            this.lastSend[ftp.id] = DateTime.Now;
                            sendToFtp(ftp.address, ftp.user, ftp.password, ftp.filename, ftp.link, ftp.externalprocess);
                            
                        }
                    }
                }
                Console.WriteLine("NEW FTP");
                
            }
        }


    }
}
