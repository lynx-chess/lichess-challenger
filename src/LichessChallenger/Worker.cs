using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using LichessChallenger.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LichessChallenger
{
    internal class Worker : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly ChallengerConfiguration _configuration;
        private readonly ILogger<Worker> _logger;

        public Worker(HttpClient httpClient, ChallengerConfiguration configuration, ILogger<Worker> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int challengeCount = 0;
            int timeControlIndex = 0;
            int botIndex = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_configuration.TimeBetweenChallenges, stoppingToken);

                // Seems repetitive, but we need to check IsPlaying status every time
                User? me = await FindUser(_configuration.BotName, stoppingToken);
                if (me is null || !me.IsOnline || me.IsPlaying)
                {
                    continue;
                }

                // Making sure to update the indexes after the 'continue' as a consequence of the engine being playing
                botIndex = (botIndex + 1) % _configuration.BotsToPlay.Count;
                if (botIndex == 0)
                {
                    timeControlIndex = (timeControlIndex + 1) % _configuration.TimeControlsToPlay.Count;
                }

                var timeControl = _configuration.TimeControlsToPlay[timeControlIndex];
                var rival = _configuration.BotsToPlay[botIndex];
                var rivalUsername = rival.Username;

                await Task.Delay(_configuration.TimeBetweenFindUserRequests, stoppingToken);

                _logger.LogInformation($"Checking on {rivalUsername} ({botIndex}/{_configuration.BotsToPlay.Count})...");
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
