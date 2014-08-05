using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using io.ebu.eis.canvasgenerator;
using io.ebu.eis.datastructures;
using io.ebu.eis.data.s3;

namespace io.ebu.eis.contentmanager
{
    public class ManagerImageReference : INotifyPropertyChanged
    {
        private HTMLRenderer _renderer;
        private CMConfigurationSection _config;

        public ManagerImageReference(HTMLRenderer renderer, CMConfigurationSection config)
        {
            _renderer = renderer;
            _config = config;
        }

        private string _template;
        public string Template { get { return _template; } set { _template = value; OnPropertyChanged("Template"); } }

        private string _publicImageUrl;

        public string PublicImageUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_publicImageUrl))
                {
                    MakePublic();
                }
                return _publicImageUrl;
            }
            set
            {
                _publicImageUrl = value;
                OnPropertyChanged("PublicImageUrl");
            }
        }

        private DataMessage _context;
        public DataMessage Context { get { return _context; } set { _context = value; OnPropertyChanged("Context"); } }

        private DateTime _lastUsed = DateTime.MinValue;
        public DateTime LastUsed { get { return _lastUsed; } set { _lastUsed = value; OnPropertyChanged("LastUsed"); } }

        private BitmapImage _previewImage;
        public BitmapImage PreviewImage
        {
            get
            {
                if (_previewImage == null) Render();
                return _previewImage;
            }
            set
            {
                _previewImage = value;
                OnPropertyChanged("PreviewImage");
                OnPropertyChanged("PreviewImageSource");
            }
        }
        public ImageSource PreviewImageSource { get { return PreviewImage; } }



        #region Rendering

        public void Render()
        {
            PreviewImage = renderImageWithTemplateContext();
        }

        public void Destroy()
        {
            PreviewImage = null;
        }

        private BitmapImage renderImageWithTemplateContext()
        {
            //var filename = System.IO.Path.Combine(_config.SlidesConfiguration.TemplatePath, template);
            if (!File.Exists(Template) && File.Exists(Path.Combine(_config.SlidesConfiguration.TemplatePath, Template)))
            {
                Template = Path.Combine(_config.SlidesConfiguration.TemplatePath, Template);
            }
            else
            {
                return null;
            }

            var templateHtml = File.ReadAllText(Template);

            // Replace @@values@@ with context Values
            const string pattern = "@@(.*?)@@";
            if (Context != null)
            {
                foreach (Match m in Regex.Matches(templateHtml, pattern))
                {
                    var variable = m.Groups[1].Value;
                    var matchedValue = m.Value;
                    var replaceValue = Context.GetValue(variable);
                    templateHtml = templateHtml.Replace(matchedValue, replaceValue);
                }
            }

            return _renderer.RenderHtml(templateHtml);
        }

        private void MakePublic()
        {
            // Save image to temporary location
            var encoder = new PngBitmapEncoder();   // encoder = new JpegBitmapEncoder();
            var imagePath = System.IO.Path.Combine(_config.SlidesConfiguration.TemplatePath, Guid.NewGuid().ToString() + ".png");

            encoder.Frames.Add(BitmapFrame.Create(PreviewImage));
            using (var filestream = new FileStream(imagePath, FileMode.Create))
            {
                encoder.Save(filestream);
            }

            // Upload Image to Amazon S3
            PublicImageUrl = AWSS3Uploader.Upload(
                imagePath,
                _config.S3Configuration.AWSAccessKey, _config.S3Configuration.AWSSecretKey,
                _config.S3Configuration.S3BucketName, _config.S3Configuration.S3Subfolder, _config.S3Configuration.S3PublicUriBase);

            // Delete temporary Image
            File.Delete(imagePath);
        }

        #endregion Rendering




        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
        #endregion PropertyChanged
    }
}
