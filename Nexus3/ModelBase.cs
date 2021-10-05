using Newtonsoft.Json;
using System;

namespace Nexus.Base.CosmosDBRepository
{
    public class ModelBase
    {
        public ModelBase()
        {
            DocumentType = GenerateDocumentType(GetType());
            DocumentNamespace = GenerateDocumentNamespace(GetType());
        }

        [JsonProperty(propertyName: "id")]
        public string Id { get; set; }

        [JsonProperty(propertyName: "documentType")]
        public string DocumentType { get; private set; }

        [JsonProperty(propertyName: "documentNamespace")]
        public string DocumentNamespace { get; private set; }

        [JsonProperty(propertyName: "partitionKey")]
        public string PartitionKey { get; internal set; }

        [JsonProperty(propertyName: "createdDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreatedDate { get; set; }

        [JsonProperty(propertyName: "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(propertyName: "lastUpdatedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime LastUpdatedDate { get; set; }

        [JsonProperty(propertyName: "lastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty(propertyName: "activeFlag")]
        public string ActiveFlag { get; set; }

        public class Partition { }

        public static string GenerateDocumentNamespace(System.Type t)
        {
            var ns = "";

            var currentType = t;

            while (currentType.Name != "ModelBase")
            {
                ns = currentType.Name + "." + ns;
                currentType = currentType.BaseType;
            }

            return "." + ns;
        }

        public static string GenerateDocumentType(System.Type t)
        {
            while (t.BaseType.Name != "ModelBase")
            {
                t = t.BaseType;
            }

            return t.Name;
        }
    }
}
