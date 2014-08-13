using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.datastructures
{
    public class CMConfigurationSection : ConfigurationSection
    {

        [ConfigurationProperty("SourceType", DefaultValue = "", IsRequired = false)]
        public String SourceType
        {
            get { return (String)this["SourceType"]; }
            set { this["SourceType"] = value; }
        }

        [ConfigurationProperty("DataConfiguration")]
        public DataConfiguration DataConfiguration
        {
            get { return (DataConfiguration)this["DataConfiguration"]; }
            set { this["DataConfiguration"] = value; }
        }

        [ConfigurationProperty("MQConfiguration")]
        public MQConfiguration MQConfiguration
        {
            get { return (MQConfiguration)this["MQConfiguration"]; }
            set { this["MQConfiguration"] = value; }
        }

        [ConfigurationProperty("S3Configuration")]
        public S3Configuration S3Configuration
        {
            get { return (S3Configuration)this["S3Configuration"]; }
            set { this["S3Configuration"] = value; }
        }

        [ConfigurationProperty("OutputConfigurations")]
        public OutputConfigurationCollection OutputConfigurations
        {
            get { return ((OutputConfigurationCollection)(base["OutputConfigurations"])); }
        }


        [ConfigurationProperty("SlidesConfiguration")]
        public SlidesConfiguration SlidesConfiguration
        {
            get { return (SlidesConfiguration)this["SlidesConfiguration"]; }
            set { this["SlidesConfiguration"] = value; }
        }

    }


    public class DataConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("DataFlowTypes", DefaultValue = "", IsRequired = false)]
        public String DataFlowTypes
        {
            get { return (String)this["DataFlowTypes"]; }
            set { this["DataFlowTypes"] = value; }
        }
        [ConfigurationProperty("ImageFlowTypes", DefaultValue = "", IsRequired = false)]
        public String ImageFlowTypes
        {
            get { return (String)this["ImageFlowTypes"]; }
            set { this["ImageFlowTypes"] = value; }
        }
        [ConfigurationProperty("DataBaseTypes", DefaultValue = "", IsRequired = false)]
        public String DataBaseTypes
        {
            get { return (String)this["DataBaseTypes"]; }
            set { this["DataBaseTypes"] = value; }
        }
        [ConfigurationProperty("DataBaseKey", DefaultValue = "", IsRequired = false)]
        public String DataBaseKey
        {
            get { return (String)this["DataBaseKey"]; }
            set { this["DataBaseKey"] = value; }
        }

        [ConfigurationProperty("InitialPictureFolder", DefaultValue = "", IsRequired = false)]
        public String InitialPictureFolder
        {
            get { return (String)this["InitialPictureFolder"]; }
            set { this["InitialPictureFolder"] = value; }
        }

        [ConfigurationProperty("IncomingPictureFolder", DefaultValue = "", IsRequired = false)]
        public String IncomingPictureFolder
        {
            get { return (String)this["IncomingPictureFolder"]; }
            set { this["IncomingPictureFolder"] = value; }
        }

        [ConfigurationProperty("StateConfigurationFile", DefaultValue = "state.txt", IsRequired = false)]
        public String StateConfigurationFile
        {
            get { return (String)this["StateConfigurationFile"]; }
            set { this["StateConfigurationFile"] = value; }
        }

        [ConfigurationProperty("DataItemDisplayConfigurations")]
        public DataItemDisplayCollection DataItemDisplayConfigurations
        {
            get { return ((DataItemDisplayCollection)(base["DataItemDisplayConfigurations"])); }
        }

        [ConfigurationProperty("DataPriorityConfigurations")]
        public DataPriorityCollection DataPriorityConfigurations
        {
            get { return ((DataPriorityCollection)(base["DataPriorityConfigurations"])); }
        }

        [ConfigurationProperty("OnAirCartOverrideConfiguration")]
        public OnAirCartOverrideCollection OnAirCartOverrideConfiguration
        {
            get { return ((OnAirCartOverrideCollection)(base["OnAirCartOverrideConfiguration"])); }
        }

        public DataItemDisplayConfiguration GetPathByDataType(string dataType)
        {
            if (DataItemDisplayConfigurations.Cast<DataItemDisplayConfiguration>().Count(e => e.DataType == dataType) > 0)
            {
                return DataItemDisplayConfigurations.Cast<DataItemDisplayConfiguration>().FirstOrDefault(e => e.DataType == dataType);
            }
            return new DataItemDisplayConfiguration();
        }
    }

    public class MQConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("Uri", DefaultValue = "", IsRequired = true)]
        public String Uri
        {
            get { return (String)this["Uri"]; }
            set { this["Uri"] = value; }
        }

        [ConfigurationProperty("DPExchange", DefaultValue = "", IsRequired = true)]
        public String DPExchange
        {
            get { return (String)this["DPExchange"]; }
            set { this["DPExchange"] = value; }
        }

        [ConfigurationProperty("DDExchange", DefaultValue = "", IsRequired = true)]
        public String DDExchange
        {
            get { return (String)this["DDExchange"]; }
            set { this["DDExchange"] = value; }
        }

    }

    public class S3Configuration : ConfigurationElement
    {
        [ConfigurationProperty("AWSAccessKey", DefaultValue = "", IsRequired = true)]
        public String AWSAccessKey
        {
            get { return (String)this["AWSAccessKey"]; }
            set { this["AWSAccessKey"] = value; }
        }

        [ConfigurationProperty("AWSSecretKey", DefaultValue = "", IsRequired = true)]
        public String AWSSecretKey
        {
            get { return (String)this["AWSSecretKey"]; }
            set { this["AWSSecretKey"] = value; }
        }

        [ConfigurationProperty("S3BucketName", DefaultValue = "", IsRequired = true)]
        public String S3BucketName
        {
            get { return (String)this["S3BucketName"]; }
            set { this["S3BucketName"] = value; }
        }

        [ConfigurationProperty("S3Subfolder", DefaultValue = "", IsRequired = true)]
        public String S3Subfolder
        {
            get { return (String)this["S3Subfolder"]; }
            set { this["S3Subfolder"] = value; }
        }

        [ConfigurationProperty("S3PublicUriBase", DefaultValue = "", IsRequired = true)]
        public String S3PublicUriBase
        {
            get { return (String)this["S3PublicUriBase"]; }
            set { this["S3PublicUriBase"] = value; }
        }

    }

    public class SlidesConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("TemplatePath", DefaultValue = "", IsRequired = true)]
        public String TemplatePath
        {
            get { return (String)this["TemplatePath"]; }
            set { this["TemplatePath"] = value; }
        }

        [ConfigurationProperty("CartConfigurations")]
        public CartCollection CartConfigurations
        {
            get { return ((CartCollection)(base["CartConfigurations"])); }
        }

    }

    [ConfigurationCollection(typeof(OutputConfiguration), AddItemName = "OutputConfiguration")]
    public class OutputConfigurationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new OutputConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OutputConfiguration)(element)).Name + element.ToString();
        }

        public OutputConfiguration this[int idx]
        {
            get { return (OutputConfiguration)BaseGet(idx); }
        }
    }

    public class OutputConfiguration : ConfigurationElement
    {
        public override string ToString()
        {
            // Vary by type
            return Name + " : " + Width + ":" + Height + ":" + Encoder + ":" + Quality;
        }

        [ConfigurationProperty("IsDefault", DefaultValue = false, IsRequired = false)]
        public bool IsDefault
        {
            get { return (bool)this["IsDefault"]; }
            set { this["IsDefault"] = value; }
        }

        [ConfigurationProperty("Name", DefaultValue = "DEFAULT", IsRequired = false)]
        public string Name
        {
            get { return (string)this["Name"]; }
            set { this["Name"] = value; }
        }

        [ConfigurationProperty("Width", DefaultValue = 320, IsRequired = false)]
        public int Width
        {
            get { return (int)this["Width"]; }
            set { this["Width"] = value; }
        }

        [ConfigurationProperty("Height", DefaultValue = 240, IsRequired = false)]
        public int Height
        {
            get { return (int)this["Height"]; }
            set { this["Height"] = value; }
        }

        [ConfigurationProperty("Encoder", DefaultValue = "PNG", IsRequired = false)]
        public string Encoder
        {
            get { return (string)this["Encoder"]; }
            set { this["Encoder"] = value; }
        }

        [ConfigurationProperty("Quality", DefaultValue = 100, IsRequired = false)]
        public int Quality
        {
            get { return (int)this["Quality"]; }
            set { this["Quality"] = value; }
        }

    }

    [ConfigurationCollection(typeof(OnAirCartOverrideCondition), AddItemName = "OnAirCartOverrideCondition")]
    public class OnAirCartOverrideCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new OnAirCartOverrideCondition();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OnAirCartOverrideCondition)(element)).DataPath + element.ToString();
        }

        public OnAirCartOverrideCondition this[int idx]
        {
            get { return (OnAirCartOverrideCondition)BaseGet(idx); }
        }
    }


    public class OnAirCartOverrideCondition : ConfigurationElement
    {
        public override string ToString()
        {
            // Vary by type
            return DataPath + " : " + ExpectedValue + ":" + Operator;
        }

        [ConfigurationProperty("DataPath", DefaultValue = "", IsRequired = false)]
        public string DataPath
        {
            get { return (string)this["DataPath"]; }
            set { this["DataPath"] = value; }
        }

        [ConfigurationProperty("ExpectedValue", DefaultValue = "", IsRequired = false)]
        public string ExpectedValue
        {
            get { return (string)this["ExpectedValue"]; }
            set { this["ExpectedValue"] = value; }
        }

        [ConfigurationProperty("Operator", DefaultValue = "EQUALS", IsRequired = false)]
        public string Operator
        {
            get { return (string)this["Operator"]; }
            set { this["Operator"] = value; }
        }
    }

    [ConfigurationCollection(typeof(DataPriorityConfiguration), AddItemName = "DataPriorityConfiguration")]
    public class DataPriorityCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataPriorityConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataPriorityConfiguration)(element)).DataPath + element.ToString();
        }

        public DataPriorityConfiguration this[int idx]
        {
            get { return (DataPriorityConfiguration)BaseGet(idx); }
        }
    }

    public class DataPriorityConfiguration : ConfigurationElement
    {
        public override string ToString()
        {
            // Vary by type
            return DataPath + " : " + ExpectedValue + ":" + Priority;
        }

        [ConfigurationProperty("DataPath", DefaultValue = "", IsRequired = false)]
        public string DataPath
        {
            get { return (string)this["DataPath"]; }
            set { this["DataPath"] = value; }
        }

        [ConfigurationProperty("ExpectedValue", DefaultValue = "", IsRequired = false)]
        public string ExpectedValue
        {
            get { return (string)this["ExpectedValue"]; }
            set { this["ExpectedValue"] = value; }
        }

        [ConfigurationProperty("Priority", DefaultValue = "Neglectable", IsRequired = false)]
        public string Priority
        {
            get { return (string)this["Priority"]; }
            set { this["Priority"] = value; }
        }
    }


    [ConfigurationCollection(typeof(DataItemDisplayConfiguration), AddItemName = "DataItemDisplayConfiguration")]
    public class DataItemDisplayCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataItemDisplayConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataItemDisplayConfiguration)(element)).DataType + element.ToString();
        }

        public DataItemDisplayConfiguration this[int idx]
        {
            get { return (DataItemDisplayConfiguration)BaseGet(idx); }
        }
    }

    public class DataItemDisplayConfiguration : ConfigurationElement
    {
        public override string ToString()
        {
            // Vary by type
            return DataType + " : " + NamePath + ":" + CategoryPath;
        }

        [ConfigurationProperty("DataType", DefaultValue = "", IsRequired = false)]
        public string DataType
        {
            get { return (string)this["DataType"]; }
            set { this["DataType"] = value; }
        }

        [ConfigurationProperty("NamePath", DefaultValue = "TITLE", IsRequired = false)]
        public string NamePath
        {
            get { return (string)this["NamePath"]; }
            set { this["NamePath"] = value; }
        }

        [ConfigurationProperty("CategoryPath", DefaultValue = "CATEGORY", IsRequired = false)]
        public string CategoryPath
        {
            get { return (string)this["CategoryPath"]; }
            set { this["CategoryPath"] = value; }
        }

        [ConfigurationProperty("TypePath", DefaultValue = "TYPE", IsRequired = false)]
        public string TypePath
        {
            get { return (string)this["TypePath"]; }
            set { this["TypePath"] = value; }
        }

        [ConfigurationProperty("ShortPath", DefaultValue = "SHORT", IsRequired = false)]
        public string ShortPath
        {
            get { return (string)this["ShortPath"]; }
            set { this["ShortPath"] = value; }
        }

        [ConfigurationProperty("UrlPath", DefaultValue = "", IsRequired = false)]
        public string UrlPath
        {
            get { return (string)this["UrlPath"]; }
            set { this["UrlPath"] = value; }
        }

        [ConfigurationProperty("PriorityPath", DefaultValue = "PRIORITY", IsRequired = false)]
        public string PriorityPath
        {
            get { return (string)this["PriorityPath"]; }
            set { this["PriorityPath"] = value; }
        }


    }



    [ConfigurationCollection(typeof(CartConfiguration), AddItemName = "CartConfiguration")]
    public class CartCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CartConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CartConfiguration)(element)).Name;
        }

        public CartConfiguration this[int idx]
        {
            get { return (CartConfiguration)BaseGet(idx); }
        }
    }

    public class CartConfiguration : ConfigurationElement
    {
        public override string ToString()
        {
            // Vary by type
            return Name + " : ";
        }

        [ConfigurationProperty("Name", DefaultValue = "", IsRequired = false)]
        public string Name
        {
            get { return (string)this["Name"]; }
            set { this["Name"] = value; }
        }

        [ConfigurationProperty("Active", DefaultValue = false, IsRequired = false)]
        public bool Active
        {
            get { return (bool)this["Active"]; }
            set { this["Active"] = value; }
        }

        [ConfigurationProperty("EditorDefault", DefaultValue = false, IsRequired = false)]
        public bool EditorDefault
        {
            get { return (bool)this["EditorDefault"]; }
            set { this["EditorDefault"] = value; }
        }

        [ConfigurationProperty("ShowInCartList", DefaultValue = false, IsRequired = false)]
        public bool ShowInCartList
        {
            get { return (bool)this["ShowInCartList"]; }
            set { this["ShowInCartList"] = value; }
        }

        [ConfigurationProperty("Slides")]
        public SlideCollection Slides
        {
            get { return ((SlideCollection)(base["Slides"])); }
        }
    }



    [ConfigurationCollection(typeof(SlideConfiguration), AddItemName = "Slide")]
    public class SlideCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SlideConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SlideConfiguration)(element)).Filename;
        }

        public SlideConfiguration this[int idx]
        {
            get { return (SlideConfiguration)BaseGet(idx); }
        }
    }

    public class SlideConfiguration : ConfigurationElement
    {
        public override string ToString()
        {
            // Vary by type
            return Filename + " : ";
        }

        [ConfigurationProperty("Filename", DefaultValue = "", IsRequired = false)]
        public string Filename
        {
            get { return (string)this["Filename"]; }
            set { this["Filename"] = value; }
        }

        [ConfigurationProperty("CanRepeat", DefaultValue = false, IsRequired = false)]
        public bool CanRepeat
        {
            get { return (bool)this["CanRepeat"]; }
            set { this["CanRepeat"] = value; }
        }

        [ConfigurationProperty("DefaultLink", DefaultValue = "", IsRequired = false)]
        public string DefaultLink
        {
            get { return (string)this["DefaultLink"]; }
            set { this["DefaultLink"] = value; }
        }
    }


}
