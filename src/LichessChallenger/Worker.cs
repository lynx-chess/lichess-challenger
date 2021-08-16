using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using LichessChallenger.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LichessChallenger
{
    public class Worker : BackgroundService
    {
        private readonly int _timeBetweenChallenges;
        private readonly int _timeBetweenFindUserRequests;
        private readonly int _timeToHaveAChallengeAccepted;
        private readonly string _me;
        private readonly List<User> _botList;
        private readonly List<Challenge> _timeControlList;
        private readonly HttpClient _httpClient;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;

            _botList = configuration.GetSection("Bots").Get<User[]>().OrderBy(_ => Guid.NewGuid()).ToList();
            _timeControlList = configuration.GetSection("TimeControls").Get<Challenge[]>().OrderBy(_ => Guid.NewGuid()).ToList();

            _me = configuration["LICHESS_USERNAME"] ?? throw new("Missing essential config: Username");
            var meAsRival = _botList.Find(u => u.Username.Equals(_me, StringComparison.OrdinalIgnoreCase));
            if (meAsRival is not null)
            {
                _botList.Remove(meAsRival);
            }

            if (!int.TryParse(configuration["TimeBetweenChallenges"], out _timeBetweenChallenges))
            {
                _timeBetweenChallenges = 30_000;
            }
            if (!int.TryParse(configuration["TimeToHaveAChallengeAccepted"], out _timeToHaveAChallengeAccepted))
            {
                _timeToHaveAChallengeAccepted = 5_000;
            }
            if (!int.TryParse(configuration["TimeBetweenFindUserRequests"], out _timeBetweenFindUserRequests))
            {
                _timeBetweenFindUserRequests = 1_500;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int challengeCount = 0;
            int timeControlIndex = 0;
            int botIndex = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_timeBetweenChallenges, stoppingToken);

                // Seems repetitive, but we need to check IsPlaying status every time
                User? me = await FindUser(_me, stoppingToken);
                if (me is null || !me.IsOnline || me.IsPlaying)
                {
                    continue;
                }

                // Making sure to update the indexes after the 'continue' as a consequence of the engine being playing
                botIndex = (botIndex + 1) % _botList.Count;
                if (botIndex == 0)
                {
                    timeControlIndex = (timeControlIndex + 1) % _timeControlList.Count;
                }

                var timeControl = _timeControlList[timeControlIndex];
                var rival = _botList[botIndex];
                var rivalUsername = rival.Username;

                await Task.Delay(_timeBetweenFindUserRequests, stoppingToken);

                _logger.LogInformation($"Checking on {rivalUsername} ({botIndex}/{_botList.Count})...");
                rival = await FindUser(rivalUsername, stoppingToken);

                if (rival?.IsOnline != true || rival?.IsPlaying == true)
                {
                    continue;
                }

                _logger.LogInformation($"#{challengeCount,-4} Trying to challenge {rivalUsername} ({timeControl.ClockLimit / 60} + {timeControl.ClockIncrement})");

                try
                {
                    var result = await _httpClient.PostAsJsonAsync($"/api/challenge/{rivalUsername}", timeControl, stoppingToken);
                    result.EnsureSuccessStatusCode();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error challenging {rivalUsername}");
                    continue;
                }

                ++challengeCount;

                await Task.Delay(_timeToHaveAChallengeAccepted, stoppingToken);
            }
        }

        private async Task<User?> FindUser(string username, CancellationToken stoppingToken)
        {
            User? user = null;
            try
            {
                user = (await _httpClient.GetFromJsonAsync<List<User>>($"/api/users/status?ids={username}", stoppingToken))?.FirstOrDefault();
            }
            catch (Exception e) // HttpRequestException, NotSupportedException or JsonException
            {
                _logger.LogError(e, $"Error querying online status of {username}");
            }

            return user;
        }
    }
}
