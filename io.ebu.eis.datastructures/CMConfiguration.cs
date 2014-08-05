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

        [ConfigurationProperty("DataItemDisplayConfigurations")]
        public DataItemDisplayCollection DataItemDisplayConfigurations
        {
            get { return ((DataItemDisplayCollection)(base["DataItemDisplayConfigurations"])); }
        }

        public DataItemDisplayConfiguration GetPathByDataType(string dataType)
        {
            return DataItemDisplayConfigurations.Cast<DataItemDisplayConfiguration>().FirstOrDefault(e => e.DataType == dataType);
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


    [ConfigurationCollection(typeof(DataItemDisplayConfiguration), AddItemName = "DataItemDisplayConfiguration")]
    public class DataItemDisplayCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataItemDisplayConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataItemDisplayConfiguration)(element)).DataType;
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
            return DataType + " : ";
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
    }


}
