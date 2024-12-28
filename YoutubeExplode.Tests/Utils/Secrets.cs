using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace YoutubeExplode.Tests.Utils;

internal class Secrets
{
    private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .AddEnvironmentVariables()
        .Build();

    public static string? YoutubeCookiesPath => Configuration["YOUTUBE_COOKIES_PATH"];
}
