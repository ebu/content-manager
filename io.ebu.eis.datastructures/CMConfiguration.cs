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


}
