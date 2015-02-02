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
        [ConfigurationProperty("DataFlowLeftActions", DefaultValue = "Cart;Context", IsRequired = false)]
        public String DataFlowLeftActions
        {
            get { return (String)this["DataFlowLeftActions"]; }
            set { this["DataFlowLeftActions"] = value; }
        }
        [ConfigurationProperty("DataFlowRightActions", DefaultValue = "Cart;Context", IsRequired = false)]
        public String DataFlowRightActions
        {
            get { return (String)this["DataFlowRightActions"]; }
            set { this["DataFlowRightActions"] = value; }
        }
        [ConfigurationProperty("AutoCartClearTypes", DefaultValue = "", IsRequired = false)]
        public String AutoCartClearTypes
        {
            get { return (String)this["AutoCartClearTypes"]; }
            set { this["AutoCartClearTypes"] = value; }
        }
        [ConfigurationProperty("AutoCartAppendTypes", DefaultValue = "", IsRequired = false)]
        public String AutoCartAppendTypes
        {
            get { return (String)this["AutoCartAppendTypes"]; }
            set { this["AutoCartAppendTypes"] = value; }
        }
        [ConfigurationProperty("AutoCartGenerationTypes", DefaultValue = "", IsRequired = false)]
        public String AutoCartGenerationTypes
        {
            get { return (String)this["AutoCartGenerationTypes"]; }
            set { this["AutoCartGenerationTypes"] = value; }
        }
        [ConfigurationProperty("AutoEditorTypes", DefaultValue = "", IsRequired = false)]
        public String AutoEditorTypes
        {
            get { return (String)this["AutoEditorTypes"]; }
            set { this["AutoEditorTypes"] = value; }
        }
        [ConfigurationProperty("ImageFlowTypes", DefaultValue = "", IsRequired = false)]
        public String ImageFlowTypes
        {
            get { return (String)this["ImageFlowTypes"]; }
            set { this["ImageFlowTypes"] = value; }
        }
        [ConfigurationProperty("ImageFlowLeftActions", DefaultValue = "Background", IsRequired = false)]
        public String ImageFlowLeftActions
        {
            get { return (String)this["ImageFlowLeftActions"]; }
            set { this["ImageFlowLeftActions"] = value; }
        }
        [ConfigurationProperty("ImageFlowRightActions", DefaultValue = "Background", IsRequired = false)]
        public String ImageFlowRightActions
        {
            get { return (String)this["ImageFlowRightActions"]; }
            set { this["ImageFlowRightActions"] = value; }
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

        [ConfigurationProperty("DataItemConfigurations")]
        public DataItemCollection DataItemConfigurations
        {
            get { return ((DataItemCollection)(base["DataItemConfigurations"])); }
        }

        [ConfigurationProperty("DataPriorityConfigurations")]
        public DataPriorityCollection DataPriorityConfigurations
        {
            get { return ((DataPriorityCollection)(base["DataPriorityConfigurations"])); }
        }

        [ConfigurationProperty("OnAirCartAutoConfiguration")]
        public OnAirCartAutoCollection OnAirCartAutoConfiguration
        {
            get { return ((OnAirCartAutoCollection)(base["OnAirCartAutoConfiguration"])); }
        }

        public DataItemConfiguration GetPathByDataType(string dataType)
        {
            if (DataItemConfigurations.Cast<DataItemConfiguration>().Count(e => e.DataType == dataType) > 0)
            {
                return DataItemConfigurations.Cast<DataItemConfiguration>().FirstOrDefault(e => e.DataType == dataType);
            }
            return new DataItemConfiguration();
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

        [ConfigurationProperty("DefaultLink", DefaultValue = "", IsRequired = true)]
        public String DefaultLink
        {
            get { return (String)this["DefaultLink"]; }
            set { this["DefaultLink"] = value; }
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

    [ConfigurationCollection(typeof(OnAirCartAutoCondition), AddItemName = "OnAirCartAutoCondition")]
    public class OnAirCartAutoCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new OnAirCartAutoCondition();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OnAirCartAutoCondition)(element)).DataPath + element.ToString();
        }

        public OnAirCartAutoCondition this[int idx]
        {
            get { return (OnAirCartAutoCondition)BaseGet(idx); }
        }
    }


    public class OnAirCartAutoCondition : ConfigurationElement
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

        [ConfigurationProperty("Operator", DefaultValue = "EQUALS", IsRequired = false)]
        public string Operator
        {
            get { return (string)this["Operator"]; }
            set { this["Operator"] = value; }
        }

        [ConfigurationProperty("Priority", DefaultValue = "Neglectable", IsRequired = false)]
        public string Priority
        {
            get { return (string)this["Priority"]; }
            set { this["Priority"] = value; }
        }
    }


    [ConfigurationCollection(typeof(DataItemConfiguration), AddItemName = "DataItemConfiguration")]
    public class DataItemCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataItemConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataItemConfiguration)(element)).DataType + element.ToString();
        }

        public DataItemConfiguration this[int idx]
        {
            get { return (DataItemConfiguration)BaseGet(idx); }
        }
    }

    public class DataItemConfiguration : ConfigurationElement
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

        [ConfigurationProperty("DefaultCartName", DefaultValue = "", IsRequired = false)]
        public string DefaultCartName
        {
            get { return (string)this["DefaultCartName"]; }
            set { this["DefaultCartName"] = value; }
        }

        [ConfigurationProperty("DefaultTemplate", DefaultValue = "", IsRequired = false)]
        public string DefaultTemplate
        {
            get { return (string)this["DefaultTemplate"]; }
            set { this["DefaultTemplate"] = value; }
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

        [ConfigurationProperty("ItemsPerSlide", DefaultValue = 4, IsRequired = false)]
        public int ItemsPerSlide
        {
            get { return (int)this["ItemsPerSlide"]; }
            set { this["ItemsPerSlide"] = value; }
        }


        [ConfigurationProperty("DefaultLink", DefaultValue = "", IsRequired = false)]
        public string DefaultLink
        {
            get { return (string)this["DefaultLink"]; }
            set { this["DefaultLink"] = value; }
        }
    }


}
