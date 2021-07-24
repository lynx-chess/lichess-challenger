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
                services
                    .AddHttpClient<Worker>((httpClient) =>
                    {
                        httpClient.BaseAddress = new Uri("https://lichess.org/api");

                        Console.WriteLine($"****{Environment.GetEnvironmentVariable("LICHESS_API_TOKEN")}****");
                        var token = context.Configuration["LICHESS_API_TOKEN"];
                        if (string.IsNullOrEmpty(token))
                        {
                            throw new ArgumentException("Missing essential config: LICHESS_API_TOKEN");
                        }

                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    })
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                    .AddPolicyHandler(RetryPolicy());
                services.AddHostedService<Worker>(services => services.GetRequiredService<Worker>());
            });
    }

    internal static IHostBuilder ConfigureLogging(this IHostBuilder builder)
    {
        return builder
            .ConfigureLogging((hostContext, logBuilder) =>
                {
                    logBuilder
                        .AddNLogWeb(new NLogLoggingConfiguration(hostContext.Configuration.GetSection("NLog")))
                        .SetMinimumLevel(LogLevel.Trace);
                })
            .UseNLog();
    }

    private static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound || msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(65, retryAttempt)));
    }
}
