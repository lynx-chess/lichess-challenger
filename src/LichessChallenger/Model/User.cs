using System;
using System.Text.Json.Serialization;

namespace LichessChallenger.Model
{
    public class User
    {
        [JsonPropertyName("username")]
        public string Username { get; }

        [JsonPropertyName("online")]
        public bool IsOnline { get; }

        [JsonPropertyName("playing")]
        public bool? IsPlaying { get; }

        [JsonConstructor]
        public User(string username, bool online, bool playing) =>
            (Username, IsOnline, IsPlaying) = (username, online, playing);
    }
}
