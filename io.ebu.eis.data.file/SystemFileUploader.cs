using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.data.file
{
    public class SystemFileUploader
    {

        public static string Upload(string pathToLocalFile, string destinationFolder, string publicUriBase)
        {
            var fileName = Path.GetFileName(pathToLocalFile) + "";
            var destinationFilePath = Path.Combine(destinationFolder, fileName);
            // Copy the file
            File.Copy(pathToLocalFile, destinationFilePath);

            return publicUriBase + "/" + fileName;
        }

    }
}
