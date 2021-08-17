using LichessChallenger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace LichessChallenger
{
    internal class ChallengerConfiguration
    {
        public const string TokenId = "LICHESS_API_TOKEN";
        public const string UsernameId = "LICHESS_USERNAME";

        /// <summary>
        /// Min. time between /api/challenge invocations
        /// https://lichess.org/api#operation/challengeCreate
        /// </summary>
        public int TimeBetweenChallenges { get; set; } = 30_000;

        //public int TimeToHaveAChallengeAccepted { get; set; } = 5_000;    // To be potentially used to cancel an open challenge

        /// <summary>
        /// Min. time between /api/users/status invocations
        /// https://lichess.org/api#operation/apiUsersStatus
        /// </summary>
        public int TimeBetweenFindUserRequests { get; set; } = 1_500;

        public User[] WeakBots { get; set; } = null!;

        public User[] AverageBots { get; set; }

        public User[] StrongBots { get; set; }

        public bool PlayWeakBots { get; set; }

        public bool PlayAverageBots { get; set; } = true;

        public bool PlayStrongBots { get; set; }

        public bool UseRandomBotOrder { get; set; }

        public bool UseRandomTimeControlOrder { get; set; }

        public Challenge[] TimeControls { get; set; }

        [JsonIgnore]
        public string BotName { get; set; }

        [JsonIgnore]
        public List<User> BotsToPlay { get; set; }

        [JsonIgnore]
        public List<Challenge> TimeControlsToPlay { get; set; }

        public ChallengerConfiguration()
        {
            BotName = string.Empty;

            AverageBots = Array.Empty<User>();
            StrongBots = Array.Empty<User>();
            WeakBots = Array.Empty<User>();
            TimeControls = Array.Empty<Challenge>();

            BotsToPlay = new List<User>();
            TimeControlsToPlay = new List<Challenge>();
        }

        public void Setup(string botName)
        {
            BotName = botName;

            if (PlayWeakBots)
            {
                BotsToPlay.AddRange(WeakBots);
            }

            if (PlayAverageBots)
            {
                BotsToPlay.AddRange(AverageBots);
            }

            if (PlayStrongBots)
            {
                BotsToPlay.AddRange(StrongBots);
            }

            if (UseRandomBotOrder)
            {
                BotsToPlay = BotsToPlay.OrderBy(_ => Guid.NewGuid()).ToList();
            }

            TimeControlsToPlay.AddRange(TimeControls);

            if (UseRandomTimeControlOrder)
            {
                TimeControlsToPlay = TimeControlsToPlay.OrderBy(_ => Guid.NewGuid()).ToList();
            }

            var meAsRival = BotsToPlay.Find(u => u.Username.Equals(BotName, StringComparison.OrdinalIgnoreCase));
            if (meAsRival is not null)
            {
                BotsToPlay.Remove(meAsRival);
            }
        }
    }
}
