using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Documents.Spatial;
using Newtonsoft.Json;

namespace CosmosDbGeoApiDemo.Dto
{
    public class InputDto
    {
        [JsonProperty("pos")]
        public Location Location { get; set; }

        [JsonProperty("dist")]
        public double Distance { get; set; }
    }

    public class Location
    {
        [JsonProperty("lon")]
        public double Longitude { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("alt")]
        public double? Altitude { get; set; }
    }
}
