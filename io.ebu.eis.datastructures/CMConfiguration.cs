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


    //[ConfigurationCollection(typeof(TaskConfiguration), AddItemName = "WFSTask")]
    //public class WFSTaskCollection : ConfigurationElementCollection
    //{
    //    protected override ConfigurationElement CreateNewElement()
    //    {
    //        return new TaskConfiguration();
    //    }

    //    protected override object GetElementKey(ConfigurationElement element)
    //    {
    //        return ((TaskConfiguration)(element)).Name;
    //    }

    //    public TaskConfiguration this[int idx]
    //    {
    //        get { return (TaskConfiguration)BaseGet(idx); }
    //    }
    //}

    //public class TaskConfiguration : ConfigurationElement
    //{
    //    public override string ToString()
    //    {
    //        // Vary by type
    //        return TaskType + " : " + Sources + " >> TO >> " + Destinations;
    //    }

    //    [ConfigurationProperty("TaskType", DefaultValue = "FileSync", IsRequired = false)]
    //    public string TaskType
    //    {
    //        get { return (string)this["TaskType"]; }
    //        set { this["TaskType"] = value; }
    //    }

    //    [ConfigurationProperty("SyncMode", DefaultValue = "All", IsRequired = false)]
    //    public string SyncMode
    //    {
    //        get { return (string)this["SyncMode"]; }
    //        set { this["SyncMode"] = value; }
    //    }

    //    [ConfigurationProperty("LastSyncDate", DefaultValue = null, IsRequired = false)]
    //    public DateTime LastSyncDate
    //    {
    //        get { return (DateTime)this["LastSyncDate"]; }
    //        set { this["LastSyncDate"] = value; }
    //    }

    //    [ConfigurationProperty("Sources", DefaultValue = "", IsRequired = false)]
    //    public string Sources
    //    {
    //        get { return (string)this["Sources"]; }
    //        set { this["Sources"] = value; }
    //    }

    //    [ConfigurationProperty("Destinations", DefaultValue = "", IsRequired = false)]
    //    public string Destinations
    //    {
    //        get { return (string)this["Destinations"]; }
    //        set { this["Destinations"] = value; }
    //    }

    //    [ConfigurationProperty("BytesPerSecond", DefaultValue = 1024L, IsRequired = false)]
    //    public long BytesPerSecond
    //    {
    //        get { return (long)this["BytesPerSecond"]; }
    //        set { this["BytesPerSecond"] = value; }
    //    }

    //    [ConfigurationProperty("CDNLocations", DefaultValue = "", IsRequired = false)]
    //    public string CDNLocations
    //    {
    //        get { return (string)this["CDNLocations"]; }
    //        set { this["CDNLocations"] = value; }
    //    }

    //    [ConfigurationProperty("CDNBytesPerSecond", DefaultValue = 1024L, IsRequired = false)]
    //    public long CDNBytesPerSecond
    //    {
    //        get { return (long)this["CDNBytesPerSecond"]; }
    //        set { this["CDNBytesPerSecond"] = value; }
    //    }

    //    [ConfigurationProperty("DataSources")]
    //    public WFSDataSourcesCollection DataSources
    //    {
    //        get { return ((WFSDataSourcesCollection)(base["DataSources"])); }
    //    }

    //    [ConfigurationProperty("DataDestinations")]
    //    public WFSDataDestinationsCollection DataDestinations
    //    {
    //        get { return ((WFSDataDestinationsCollection)(base["DataDestinations"])); }
    //    }

    //    public string Name
    //    {
    //        get { return this.Sources + ":" + this.Destinations; }
    //    }

    //}


}
