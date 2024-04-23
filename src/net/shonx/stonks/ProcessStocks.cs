namespace net.shonx.stocks;

using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class ProcessStocks(ILoggerFactory loggerFactory)
{

    private const string NCRON_VALUE = "0 30 16 * * 1-5"; // See https://github.com/atifaziz/NCrontab
    private static readonly string? DISCORD_URL = Environment.GetEnvironmentVariable("DISCORD_URL");
    private static readonly string? API_KEY = Environment.GetEnvironmentVariable("API_KEY");
    private readonly ILogger _logger = loggerFactory.CreateLogger<ProcessStocks>();
    private static readonly HttpClient client = new();
    private static readonly string[] symbols = ["DJT", "TSLA", "NVDA", "SPY", "GLD", "SLV"];

    [Function("ProcessStocks")]
    public async Task Run([TimerTrigger(NCRON_VALUE)] TimerInfo myTimer)
    {
        if (string.IsNullOrEmpty(DISCORD_URL))
        {
            _logger.LogError("DISCORD_URL not found.");
            throw new NullReferenceException();
        }
        if (string.IsNullOrEmpty(API_KEY))
        {
            _logger.LogError("API_KEY not found.");
            throw new NullReferenceException();
        }
        DiscordMessage message = new(null);
        DiscordEmbed embed = new("Market Update", default, null);
        int positive = 0;
        foreach (string symbol in symbols)
        {
            StockData? data = await StockData(symbol);
            if (data is null || data.TimeSeries is null)
            {
                _logger.LogError("Data returned null.");
                return;
            }
            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");

            var todayPair = data.TimeSeries.ElementAt(0);
            if (!todayPair.Key.Equals(todaysDate))
            {
                _logger.LogInformation("Market is closed.");
                return;
            }
            DailyData today = todayPair.Value;
            DailyData yesterday = data.TimeSeries.ElementAt(1).Value;

            decimal opened = yesterday.Close;
            decimal closed = today.Close;

            decimal changedValue = closed - opened;

            decimal changedPercent = changedValue / opened * 100;

            string emoji = GetEmoji(changedPercent);

            string verb;
            if (changedPercent >= 0)
            {
                verb = "Up";
                positive++;
            }
            else
            {
                verb = "Down";
                positive--;
            }
            
            if ((changedPercent > (decimal)-0.1) && (changedPercent < (decimal)0.1))
            {
                embed.Fields.Add(new(symbol, $"{verb} {emoji} {changedPercent:F4}% to ${closed:F2}"));
            }
            else
            {
                embed.Fields.Add(new(symbol, $"{verb} {emoji} {changedPercent:F2}% to ${closed:F2}"));
            }
        }
        message.Embeds.Add(embed);
        if (positive >= 0)
        {
            embed.Color = 65280;
        }
        else
        {
            embed.Color = 16711680;
        }

        await SendToDiscord(message);
    }

    private static string GetEmoji(decimal percent)
    {
        if (percent >= 7)
            return "🚀";
        if (percent >= 0 && percent < 7)
            return "📈";
        if (percent > -7 && percent < 0)
            return "📉";
        return "🔥";
    }

    private static async Task<StockData?> StockData(string symbol)
    {
        string URI = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=compact&apikey={API_KEY}";
        HttpRequestMessage request = new(HttpMethod.Get, URI);

        using HttpResponseMessage response = await client.SendAsync(request);
        int statusCode = (int)response.StatusCode;
        if (statusCode != 200)
        {
            Console.WriteLine($"Got {statusCode}");
            return null;
        }
        string apiResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<StockData>(apiResponse);
    }

    private static async Task SendToDiscord(DiscordMessage message)
    {
        string JSON = JsonConvert.SerializeObject(message);
        HttpRequestMessage request = new(HttpMethod.Post, DISCORD_URL)
        {
            Content = new StringContent(JSON, Encoding.UTF8, "application/json")
        };

        using HttpResponseMessage response = await client.SendAsync(request);
        int statusCode = (int)response.StatusCode;
        if (statusCode != 204)
        {
            Console.WriteLine("Discord Error");
            return;
        }
    }

}