using LichessChallenger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Http;
using Polly;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using NLog.Web;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Configuration;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices()
    .ConfigureLogging()
    .Build();

try
{
    host.Run();
}
catch (Exception e)
{
    var message = $"Critical failure in LichessChallenger: {e.Message}";
    Console.WriteLine(message);
}

internal static class HostBuilderExtensions
{
    internal static IHostBuilder ConfigureServices(this IHostBuilder builder)
    {
        return builder
            .ConfigureServices((context, services) =>
            {
                var challengerConfiguration = new ChallengerConfiguration();
                context.Configuration.GetRequiredSection(nameof(ChallengerConfiguration)).Bind(challengerConfiguration);

                var token = context.Configuration[ChallengerConfiguration.TokenId];
                if (string.IsNullOrEmpty(token))
                {
                    throw new ArgumentException($"Missing essential config: {ChallengerConfiguration.TokenId}");
                }

                var botName = context.Configuration[ChallengerConfiguration.UsernameId];
                if (string.IsNullOrEmpty(botName))
                {
                    throw new ArgumentException($"Missing essential config: {ChallengerConfiguration.UsernameId}");
                }

                challengerConfiguration.Setup(botName);

                services.AddSingleton(challengerConfiguration);

                services
                    .AddHttpClient<Worker>((httpClient) =>
                    {
                        httpClient.BaseAddress = new Uri("https://lichess.org/api");
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    })
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                    .AddPolicyHandler(ErrorRetryPolicy())
                    .AddPolicyHandler(TooManyRequestsRetryPolicy());

                services.AddHostedService(services => services.GetRequiredService<Worker>());
            });
    }

    internal static IHostBuilder ConfigureLogging(this IHostBuilder builder)
    {
        return builder
            .ConfigureLogging((hostContext, logBuilder) =>
                {
                    logBuilder
                        .AddNLogWeb(new NLogLoggingConfiguration(hostContext.Configuration.GetRequiredSection("NLog")))
                        .SetMinimumLevel(LogLevel.Trace);
                })
            .UseNLog();
    }

    private static IAsyncPolicy<HttpResponseMessage> ErrorRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> TooManyRequestsRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .HandleResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(65));
    }
}
