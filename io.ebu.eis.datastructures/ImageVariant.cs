using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace io.ebu.eis.datastructures
{
    /// <summary>
    /// Matches the DataMessage case class in the DataGateway project
    /// https://github.com/ebu/DataGateway
    /// </summary>
    [DataContract]
    public class ImageVariant
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

    }
}
