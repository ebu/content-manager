using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
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
using System.Runtime.Serialization;
using io.ebu.eis.datastructures.Plain.Collections;

namespace io.ebu.eis.contentmanager
{
    [DataContract]
    public class ManagerImageReference : INotifyPropertyChanged
    {
        internal HTMLRenderer _renderer;
        internal CMConfigurationSection _config;

        public ManagerImageReference(HTMLRenderer renderer, CMConfigurationSection config)
        {
            TemplateFields = new DispatchedObservableCollection<ManagerTemplateField>();
            _renderer = renderer;
            _config = config;
            IndexOffset = 0;
            ImageVariants = new List<ImageVariant>();
        }


        private string _template;
        [DataMember(Name = "template")]
        public string Template { get { return _template; } set { _template = value; ReadTemplateFields(); OnPropertyChanged("Template"); } }


        private string _background;
        [DataMember(Name = "background")]
        public string Background { get { return _background; } set { _background = value; OnPropertyChanged("Background"); } }


        private string _link;
        [DataMember(Name = "link")]
        public string Link { get { return _link; } set { _link = value; OnPropertyChanged("Link"); } }


        private List<ImageVariant> _imageVariants;
        [DataMember(Name = "imagevariants")]
        public List<ImageVariant> ImageVariants { get { return _imageVariants; } set { _imageVariants = value; OnPropertyChanged("ImageVariants"); } }


        private bool _isActive;
        [DataMember(Name = "isactive")]
        public bool IsActive { get { return _isActive; } set { _isActive = value; OnPropertyChanged("IsActive"); } }

        private int _indexOffset;
        [DataMember(Name = "indexoffset")]
        public int IndexOffset { get { return _indexOffset; } set { _indexOffset = value; OnPropertyChanged("IndexOffset"); } }

        private bool _canRepeate;
        [DataMember(Name = "canrepeate")]
        public bool CanRepeate { get { return _canRepeate; } set { _canRepeate = value; OnPropertyChanged("CanRepeate"); } }


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
        [DataMember(Name = "context")]
        public DataMessage Context { get { return _context; } set { _context = value; ReadTemplateFields(); OnPropertyChanged("Context"); } }

        private DispatchedObservableCollection<ManagerTemplateField> _tpFields;
        [DataMember(Name = "templatefields")]
        public DispatchedObservableCollection<ManagerTemplateField> TemplateFields { get { return _tpFields; } set { _tpFields = value; OnPropertyChanged("TemplateFields"); } }


        private long _lastUsed = 0;
        public DateTime LastUsed { get { return DateTime.FromBinary(_lastUsed); } set { _lastUsed = value.ToBinary(); OnPropertyChanged("LastUsed"); OnPropertyChanged("LastUsedBinary"); } }
        [DataMember(Name = "lastused")]
        public long LastUsedBinary { get { return _lastUsed; } set { _lastUsed = value; OnPropertyChanged("LastUsed"); OnPropertyChanged("LastUsedBinary"); } }

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
            PublicImageUrl = null;
            PreviewImage = renderImageWithTemplateContext();
        }

        public void Destroy()
        {
            PreviewImage = null;
        }

        private void ReadTemplateFields()
        {
            if (Template == null || TemplateFields == null)
            {
                return;
            }
            if (!File.Exists(Template) && File.Exists(Path.Combine(_config.SlidesConfiguration.TemplatePath, Template)))
            {
                Template = Path.Combine(_config.SlidesConfiguration.TemplatePath, Template);
            }
            else if (!File.Exists(Template))
            {
                return;
            }

            var templateHtml = File.ReadAllText(Template);

            // Replace @@values@@ with context Values
            const string pattern = "@@(.*?)@@";

            TemplateFields = new DispatchedObservableCollection<ManagerTemplateField>();
            foreach (Match m in Regex.Matches(templateHtml, pattern))
            {
                var variable = m.Groups[1].Value;
                var matchedValue = m.Value;
                var replaceValue = "";
                if (Context != null)
                {
                    replaceValue = Context.GetValue(variable);
                }
                // Add all fields to template fields
                if (TemplateFields.Count(x => x.Title == variable) == 0)
                {
                    // Only add once to list
                    TemplateFields.Add(new ManagerTemplateField(variable, replaceValue));
                }
            }
        }

        private BitmapImage renderImageWithTemplateContext()
        {
            //var filename = System.IO.Path.Combine(_config.SlidesConfiguration.TemplatePath, template);
            if (Template == null || TemplateFields == null || _renderer == null)
            {
                return null;
            }
            if (!File.Exists(Template) && File.Exists(Path.Combine(_config.SlidesConfiguration.TemplatePath, Template)))
            {
                Template = Path.Combine(_config.SlidesConfiguration.TemplatePath, Template);
            }
            else if (!File.Exists(Template))
            {
                return null;
            }

            var templateHtml = File.ReadAllText(Template);

            // Replace @@values@@ with context Values
            const string pattern = "@@(.*?)@@";
            foreach (Match m in Regex.Matches(templateHtml, pattern))
            {
                var variable = m.Groups[1].Value;
                var matchedValue = m.Value;
                var replaceField = TemplateFields.FirstOrDefault(x => x.Title == variable);
                var replaceValue = "";
                if (replaceField != null)
                    replaceValue = replaceField.Value;
                templateHtml = templateHtml.Replace(matchedValue, replaceValue);
            }

            // Replace ##index## with offseted Values
            const string indexpattern = "##(.*?)##";
            foreach (Match m in Regex.Matches(templateHtml, indexpattern))
            {
                var variable = Convert.ToInt32(m.Groups[1].Value);
                var matchedValue = m.Value;
                var replaceValue = (IndexOffset + variable).ToString();
                templateHtml = templateHtml.Replace(matchedValue, replaceValue);
            }

            // Background Management
            const string bgpattern = "\"backgroundimage\": ?\"(?<image>.*?)\"";
            Match mbg = Regex.Match(templateHtml, bgpattern);
            if (mbg.Success && !String.IsNullOrEmpty(Background))
            {
                var variable = mbg.Groups[1].Value;
                var filepath = Background.Replace("\\", "/"); //"file://" +
                templateHtml = templateHtml.Replace(variable, filepath);
            }


            return _renderer.RenderHtml(templateHtml);

        }

        private void MakePublic()
        {
            // Reset Variants
            ImageVariants = new List<ImageVariant>(); // zFWx@-djeG6

            // Save image to temporary location
            foreach (OutputConfiguration output in _config.OutputConfigurations)
            {
                var imagePath = "";

                if (output.Encoder == "PNG")
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();

                    imagePath = Path.Combine(_config.SlidesConfiguration.TemplatePath, Guid.NewGuid().ToString() + ".png");

                    encoder.Frames.Add(BitmapFrame.Create(PreviewImage));
                    using (var filestream = new FileStream(imagePath, FileMode.Create))
                    {
                        encoder.Save(filestream);
                    }
                }
                if (output.Encoder == "JPEG")
                {
                    //encoder = new JpegBitmapEncoder();
                    //((JpegBitmapEncoder)encoder).QualityLevel = output.Quality;
                    //extention = ".jpg";
                    // Compress IMAGE
                    var encoder = GetEncoder(ImageFormat.Jpeg);

                    // Create an Encoder object based on the GUID
                    // for the Quality parameter category.
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Compression;
                    System.Drawing.Imaging.Encoder myEncoder2 = System.Drawing.Imaging.Encoder.Quality;


                    // Create an EncoderParameters object.
                    // An EncoderParameters object has an array of EncoderParameter
                    // objects. In this case, there is only one
                    // EncoderParameter object in the array.
                    var myEncoderParameters = new EncoderParameters(2);

                    var myEncoderParameter = new EncoderParameter(myEncoder, 0L);
                    var myEncoderParameter2 = new EncoderParameter(myEncoder2, output.Quality);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    myEncoderParameters.Param[1] = myEncoderParameter2;

                    imagePath = Path.Combine(_config.SlidesConfiguration.TemplatePath, Guid.NewGuid().ToString() + ".jpg");

                    using (var outStream = new MemoryStream())
                    {
                        BitmapEncoder enc = new BmpBitmapEncoder();
                        enc.Frames.Add(BitmapFrame.Create(PreviewImage));
                        enc.Save(outStream);
                        var bitmap = new Bitmap(outStream);

                        bitmap.Save(imagePath, encoder, myEncoderParameters);
                    }

                }
                
                // Upload Image to Amazon S3
                var publicUrl = AWSS3Uploader.Upload(imagePath, _config.S3Configuration.AWSAccessKey,
                        _config.S3Configuration.AWSSecretKey, _config.S3Configuration.S3BucketName,
                        _config.S3Configuration.S3Subfolder, _config.S3Configuration.S3PublicUriBase);

                // Add to variants
                ImageVariants.Add(new ImageVariant() { Name = output.Name, Url = publicUrl });

                // Save the Url
                if (output.Name == "DEFAULT")
                {
                    PublicImageUrl = publicUrl;
                }

                // Delete temporary Image
                File.Delete(imagePath);
            }

            if (string.IsNullOrEmpty(_publicImageUrl))
            {
                // TODO No default thus choose one
            }
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

        #endregion Rendering

        public ManagerImageReference Clone()
        {
            // Serialize
            var js = JsonSerializer.Serialize(this);
            // Deserialize
            var clone = JsonSerializer.Deserialize<ManagerImageReference>(js);
            // Reset instance values
            clone._config = _config;
            clone._renderer = _renderer;
            // Return
            return clone;
        }


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
