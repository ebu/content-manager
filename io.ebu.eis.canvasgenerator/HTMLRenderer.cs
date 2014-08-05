using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace io.ebu.eis.canvasgenerator
{
    public class HTMLRenderer
    {
        private string _pathToExe = "phantomjs.exe";
        private string _pathToWorkingDir = "templates/";
        private string _phantomPageGenerator = "../sliderenderer.js";
        private string _phantomHtmlGenerator = "../htmlrenderer.js";
        private string _phantomArgumentProperties = "320px*240px";
        private int _timeToExit = 500;

        public HTMLRenderer(string templateDirectory)
        {
            _pathToWorkingDir = templateDirectory;
        }

        public BitmapImage Render(string file)
        {
            var args = String.Format("{0} {1} {2}", _phantomPageGenerator, file, _phantomArgumentProperties);
            var startInfo = new ProcessStartInfo
            {
                FileName = _pathToExe,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WorkingDirectory = _pathToWorkingDir // PDF Tool Path
            };
            var p = new Process();
            p.StartInfo = startInfo;
            p.Start();
            p.WaitForExit(_timeToExit);
            //Read the Error:
            //string error = p.StandardError.ReadToEnd();
            //Read the Output:
            var base64image = p.StandardOutput.ReadToEnd().Trim();
            var bytes = Convert.FromBase64CharArray(base64image.ToCharArray(), 0, base64image.Length);
            var image = GetBitmapImage(bytes);
            return image;
        }

        public BitmapImage RenderHtml(string html)
        {
            // TODO Pass as argument and decode
            var tempFilename = Guid.NewGuid().ToString()+".html";
            var tempfile = Path.Combine(_pathToWorkingDir, tempFilename);
            var file = File.Create(tempfile);
            var content = Encoding.UTF8.GetBytes(html);
            file.Write(content, 0, content.Length);
            file.Flush();
            file.Close();
            //var htmlEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));
            //var args = String.Format("{0} {1} {2}", _phantomHtmlGenerator, htmlEncoded, _phantomArgumentProperties);
            //var startInfo = new ProcessStartInfo
            //{
            //    FileName = _pathToExe,
            //    Arguments = args,
            //    UseShellExecute = false,
            //    CreateNoWindow = true,
            //    RedirectStandardOutput = true,
            //    RedirectStandardError = true,
            //    RedirectStandardInput = true,
            //    WorkingDirectory = _pathToWorkingDir // PDF Tool Path
            //};
            //var p = new Process();
            //p.StartInfo = startInfo;
            //p.Start();
            //p.WaitForExit(_timeToExit);
            ////Read the Error:
            ////string error = p.StandardError.ReadToEnd();
            ////Read the Output:
            //var base64image = p.StandardOutput.ReadToEnd().Trim();
            //var bytes = Convert.FromBase64CharArray(base64image.ToCharArray(), 0, base64image.Length);
            //var image = GetBitmapImage(bytes);

            // TODO This is ugly but works : )
            var image = Render(System.IO.Path.GetFileName(tempfile));
            
            // TODO Remove
            // Delete Temp file
            File.Delete(tempfile);

            return image;
        }

        public static BitmapImage GetBitmapImage(byte[] imageBytes)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageBytes);
            bitmapImage.EndInit();
            return bitmapImage;
        }


    }
}
