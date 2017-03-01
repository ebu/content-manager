using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace io.ebu.eis.data.ftp
{
    public class FtpFileUploader
    {
        public static string Upload(string pathToLocalFile, string ftpServer, string ftpUsername, string ftpPassword,
            string ftpSubfolder, string uniqueImage, string publicUriBase)
        {
            var fileName = Path.GetFileName(pathToLocalFile) + "";

            // Handle Unique Filename Upload
            if (!string.IsNullOrEmpty(uniqueImage))
            {
                fileName = uniqueImage;
            }

            var tmpFileName = fileName.Substring(0, fileName.Length - 2);
            var tempUri = new Uri("ftp://"+ftpServer+"/"+ftpSubfolder+"/"+tmpFileName);

            var request = (FtpWebRequest)WebRequest.Create(tempUri);
            request.Proxy = null;
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Authentication
            if (!string.IsNullOrEmpty(ftpUsername))
            {
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            }

            // Copy the contents of the file to the request stream.
            var stream = new FileStream(pathToLocalFile, FileMode.Open);
            var reader = new BinaryReader(stream);
            var fileContents = reader.ReadBytes((int)stream.Length);
            stream.Close();
            reader.Close();

            request.ContentLength = fileContents.Length;
            try
            {
                var requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();
            }
            catch (WebException e)
            {
                Console.WriteLine("error " + e.Message);
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry($"EIS Content Manager failed to publish the image to FTP.\n{e.Message}\n\n{e.StackTrace}", EventLogEntryType.Error, 101, 1);
                }
            }

            // Rename File
            FtpWebResponse response = null;
            try
            {
                response = (FtpWebResponse)request.GetResponse();
                request = (FtpWebRequest)WebRequest.Create(tempUri);
                request.Proxy = null;
                // Authentication
                if (!string.IsNullOrEmpty(ftpUsername))
                {
                    request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
                }
                request.Method = WebRequestMethods.Ftp.Rename;

                request.RenameTo = fileName;
                response = (FtpWebResponse)request.GetResponse();

            }
            catch (WebException e)
            {
                Console.WriteLine("error " + e.Message);
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry($"EIS Content Manager failed to publish the image to FTP and rename it.\n{e.Message}\n\n{e.StackTrace}", EventLogEntryType.Error, 101, 1);
                }
            }

            if (response != null)
            {
                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
                response.Close();
            }

            return publicUriBase + "/" + fileName;
        }
    }
}
