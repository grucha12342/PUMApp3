using Newtonsoft.Json;
using SQLite;

namespace App6
{
    [JsonObject]
    public class WeatherObservation
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        [JsonProperty("ICAO")]
        public string ICAO { get; set; }
        [JsonProperty("clouds")]
        public string clouds { get; set; }
        [JsonProperty("cloudsCode")]
        public string cloudsCode { get; set; }
        [JsonProperty("countryCode")]
        public string countryCode { get; set; }
        [JsonProperty("datetime")]
        public string datetime { get; set; }
        [JsonProperty("dewPoint")]
        public string dewPoint { get; set; }
        [JsonProperty("elevation")]
        public int elevation { get; set; }
        [JsonProperty("hectoPascAltimeter")]
        public int hectoPascAltimeter { get; set; }
        [JsonProperty("humidity")]
        public int humidity { get; set; }
        [JsonProperty("lat")]
        public double lat { get; set; }
        [JsonProperty("lng")]
        public double lng { get; set; }
        [JsonProperty("observation")]
        public string observation { get; set; }
        [JsonProperty("stationName")]
        public string stationName { get; set; }
        [JsonProperty("temperature")]
        public string temperature { get; set; }
        [JsonProperty("weatherCondition")]
        public string weatherCondition { get; set; }
        [JsonProperty("weatherConditionCode")]
        public string weatherConditionCode { get; set; }
        [JsonProperty("windDirection")]
        public int windDirection { get; set; }
        [JsonProperty("windSpeed")]
        public string windSpeed { get; set; }
    }

    public class RootObject
    {
        public WeatherObservation weatherObservation { get; set; }
    }
}