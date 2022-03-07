using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cybersource.Models
{
    public class Manifest
    {   
        [JsonProperty("customFields", NullValueHandling = NullValueHandling.Ignore)]
        public List<CustomField> CustomFields { get; set; }
    }

    public partial class CustomField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
