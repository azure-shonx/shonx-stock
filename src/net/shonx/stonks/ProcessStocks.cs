namespace net.shonx.stocks;

using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class ProcessStocks(ILoggerFactory loggerFactory)
{

    private static readonly string? DISCORD_URL = Environment.GetEnvironmentVariable("DISCORD");
    private static readonly string? API_KEY = Environment.GetEnvironmentVariable("AV_KEY");
    private readonly ILogger _logger = loggerFactory.CreateLogger<ProcessStocks>();
    private static readonly HttpClient client = new();
    private static readonly string[] symbols = ["DJT", "TSLA", "NVDA", "SPY", "GLD", "SLV"];

    [FunctionName("ProcessStocks")]
    public async Task Run([TimerTrigger("30 16 * * *")] TimerInfo myTimer)
    {
        if (string.IsNullOrEmpty(DISCORD_URL))
            throw new NullReferenceException();
        if (string.IsNullOrEmpty(API_KEY))
            throw new NullReferenceException();
        DiscordMessage message = new(null);
        DiscordEmbed embed = new("Market Update", default, null);
        int positive = 0;
        foreach (string symbol in symbols)
        {
            StockData? data = await StockData(symbol);
            if (data is null || data.TimeSeries is null)
            {
                Console.WriteLine("Data returned null.");
                return;
            }
            // string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");
            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");

            var todayPair = data.TimeSeries.ElementAt(0);
            if (!todayPair.Key.Equals(todaysDate))
            {
                Console.WriteLine("Market is closed.");
                return; // Market's closed
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

            Console.WriteLine($"opened={opened:F2} closed={closed:F2} changedValue={changedValue:F2} changedPercent={changedPercent}");
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