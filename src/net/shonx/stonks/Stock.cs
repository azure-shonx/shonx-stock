using Newtonsoft.Json;

namespace net.shonx.stocks
{
    public class StockData
    {
        [JsonProperty("Meta Data")]
        public MetaData? MetaData { get; set; }

        [JsonProperty("Time Series (Daily)")]
        public Dictionary<string, DailyData>? TimeSeries { get; set; }
    }

    public class MetaData
    {
        [JsonProperty("1. Information")]
        public string? Information { get; set; }

        [JsonProperty("2. Symbol")]
        public string? Symbol { get; set; }

        [JsonProperty("3. Last Refreshed")]
        public DateTime LastRefreshed { get; set; }

        [JsonProperty("4. Output Size")]
        public string? OutputSize { get; set; }

        [JsonProperty("5. Time Zone")]
        public string? TimeZone { get; set; }
    }

    public class DailyData
    {
        [JsonProperty("1. open")]
        public decimal Open { get; set; }

        [JsonProperty("2. high")]
        public decimal High { get; set; }

        [JsonProperty("3. low")]
        public decimal Low { get; set; }

        [JsonProperty("4. close")]
        public decimal Close { get; set; }

        [JsonProperty("5. volume")]
        public long Volume { get; set; }
    }
}