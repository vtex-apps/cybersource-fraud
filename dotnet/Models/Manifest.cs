﻿using Newtonsoft.Json;
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

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public List<Option> Options { get; set; }
    }

    public partial class Option
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
