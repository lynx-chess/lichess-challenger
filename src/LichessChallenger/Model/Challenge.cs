using System;
using System.Text.Json.Serialization;

namespace LichessChallenger.Model
{
    public class Challenge
    {
        [JsonPropertyName("clock.limit")]
        public int ClockLimit { get; init; }

        [JsonPropertyName("clock.increment")]
        public int ClockIncrement { get; init; }

        [JsonPropertyName("color")]
        public string Color { get; init; } = "random";

        [JsonPropertyName("variant")]
        public string Variant { get; init; } = "standard";

        [JsonPropertyName("rated")]
        public bool Rated { get; init; } = true;
    }
}
