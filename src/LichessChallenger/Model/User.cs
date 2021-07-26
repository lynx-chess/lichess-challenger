using System.Text.Json.Serialization;

namespace LichessChallenger.Model
{
    public class User
    {
        [JsonPropertyName("name")]
        public string Username { get; init; } = null!;

        [JsonPropertyName("online")]
        public bool IsOnline { get; init; }

        [JsonPropertyName("playing")]
        public bool IsPlaying { get; init; }
    }
}
