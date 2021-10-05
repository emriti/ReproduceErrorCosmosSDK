using Newtonsoft.Json;

namespace Nexus.Base.CosmosDBRepositoryTests.DAL
{
    public class Instructor {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
        
        // binusianId / PersonCode
        [JsonProperty(PropertyName = "userCode")]
        public string UserCode { get; set; }

        [JsonProperty(PropertyName = "pictureUrl")]
        public string PictureUrl { get; set; }

        [JsonProperty(PropertyName = "subRoleTypeId")]
        public string SubRoleTypeId { get; set; }

        [JsonProperty(PropertyName = "subRoleTypeDescription")]
        public string SubRoleTypeDescription { get; set; }

        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "personCode")]
        public string PersonCode { get; set; }

        // true | false  2021-08-04 @ss define instructor from enrollment service or not
        [JsonProperty(PropertyName = "isEnrolled")]
        public bool IsEnrolled { get; set; }
    }
}
