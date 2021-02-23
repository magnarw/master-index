
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace master_index_data_access.datamodel
{
    public class Master
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "parititionKey")]
        public string ParititionKey { get; set; }

        
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public string Enitiy { get; set; }

        public List<MasterIndexRecord> MasterIndex { get; set; }
    }

    public class MasterIndexRecord
    {
        public string System { get; set; }
        public string SystemId { get; set; }
    }

    public class MasterIndexRelation
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "parititionKey")]
        public string ParititionKey { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        
        public string MasterId { get; set; }
    }
}