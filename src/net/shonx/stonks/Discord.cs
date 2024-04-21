using Newtonsoft.Json;

namespace net.shonx.stocks
{

    public class DiscordMessage(List<DiscordEmbed>? Embeds)
    {
        [JsonProperty("embeds")]
        public List<DiscordEmbed> Embeds { get; set; } = Embeds ?? [];
    }

    public class DiscordEmbed(string Title, int Color, List<DiscordField>? Fields)
    {
        [JsonProperty("title")]
        public string? Title { get; set; } = Title;

        [JsonProperty("color")]
        public int Color { get; set; } = Color;

        [JsonProperty("fields")]
        public List<DiscordField> Fields { get; set; } = Fields ?? [];
    }

    public class DiscordField(string name, string value)
    {
        [JsonProperty("name")]
        public string Name { get; set; } = name;

        [JsonProperty("value")]
        public string Value { get; set; } = value;
    }
}