using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Documents.Spatial;
using Newtonsoft.Json;

namespace CosmosDbGeoApiDemo.Dto
{
    public class PoiDto
    {
        [JsonProperty("poi_id")]
        public string PoiId { get; set; }

        public string Name { get; set; }
        public Point Location { get; set; }
    }
}
