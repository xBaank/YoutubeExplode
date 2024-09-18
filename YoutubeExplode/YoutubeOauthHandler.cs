using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode;

[JsonSerializable(typeof(TokenResponse))]
[JsonSerializable(typeof(DeviceCodeResponse))]
internal partial class MyJsonContext : JsonSerializerContext { }

/// <summary>
/// Oauth2 login handler
/// </summary>
public class YouTubeOAuth2Handler
{
    private static readonly string _CLIENT_ID =
        "861556708454-d6dlm3lh05idd8npek18k6be8ba3oc68.apps.googleusercontent.com";
    private static readonly string _CLIENT_SECRET = "SboVhoG9s0rNafixCSGGKXAT";
    private static readonly string _SCOPES =
        "http://gdata.youtube.com https://www.googleapis.com/auth/youtube";
    private const string TOKEN_URL = "https://www.youtube.com/o/oauth2/token";
    private const string DEVICE_CODE_URL = "https://www.youtube.com/o/oauth2/device/code";
    private readonly HttpClient _client = new();

    private TokenData? _tokenData;

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public async Task<TokenData> InitializeOAuthAsync()
    {
        _tokenData = await GetTokenAsync();
        if (_tokenData == null || !ValidateTokenData(_tokenData))
        {
            _tokenData = await AuthorizeAsync();
            StoreToken(_tokenData);
        }

        if (_tokenData.Expires < DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60)
        {
            Console.WriteLine("Access token expired, refreshing");
            _tokenData = await RefreshTokenAsync(_tokenData.RefreshToken);
            StoreToken(_tokenData);
        }

        return _tokenData;
    }

    private bool ValidateTokenData(TokenData tokenData)
    {
        return tokenData != null
            && !string.IsNullOrEmpty(tokenData.AccessToken)
            && tokenData.Expires > 0
            && !string.IsNullOrEmpty(tokenData.RefreshToken);
    }

    private async Task<TokenData> RefreshTokenAsync(string refreshToken)
    {
        var postData = new Dictionary<string, string>
        {
            { "client_id", _CLIENT_ID },
            { "client_secret", _CLIENT_SECRET },
            { "refresh_token", refreshToken },
            { "grant_type", "refresh_token" },
        };

        var response = await _client.PostAsync(TOKEN_URL, new FormUrlEncodedContent(postData));
        var jsonResponse = await response.Content.ReadAsStreamAsync();

        var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(
            jsonResponse,
            MyJsonContext.Default.TokenResponse
        );
        if (tokenResponse is null || tokenResponse.Error is null)
        {
            Console.WriteLine(
                $"Failed to refresh access token: {tokenResponse?.Error}. Restarting authorization flow"
            );
            return await AuthorizeAsync();
        }

        return new TokenData
        {
            AccessToken = tokenResponse.AccessToken,
            Expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tokenResponse.ExpiresIn,
            TokenType = tokenResponse.TokenType,
            RefreshToken = tokenResponse.RefreshToken ?? refreshToken,
        };
    }

    private async Task<TokenData> AuthorizeAsync()
    {
        var deviceRequestData = new Dictionary<string, string>
        {
            { "client_id", _CLIENT_ID },
            { "scope", _SCOPES },
            { "device_id", Guid.NewGuid().ToString("N") },
            { "device_model", "ytlr::" },
        };

        var response = await _client.PostAsync(
            DEVICE_CODE_URL,
            new FormUrlEncodedContent(deviceRequestData)
        );
        var jsonResponse = await response.Content.ReadAsStreamAsync();
        var codeResponse =
            await JsonSerializer.DeserializeAsync(
                jsonResponse,
                MyJsonContext.Default.DeviceCodeResponse
            ) ?? throw new YoutubeExplodeException("Code response is null");

        Console.WriteLine(
            $"To give access to your account, go to {codeResponse.VerificationUrl} and enter code {codeResponse.UserCode}"
        );

        while (true)
        {
            var tokenRequestData = new Dictionary<string, string>
            {
                { "client_id", _CLIENT_ID },
                { "client_secret", _CLIENT_SECRET },
                { "code", codeResponse.DeviceCode },
                { "grant_type", "http://oauth.net/grant_type/device/1.0" },
            };

            var tokenResponse = await _client.PostAsync(
                TOKEN_URL,
                new FormUrlEncodedContent(tokenRequestData)
            );
            var tokenJsonResponse = await tokenResponse.Content.ReadAsStreamAsync();
            var token =
                await JsonSerializer.DeserializeAsync(
                    tokenJsonResponse,
                    MyJsonContext.Default.TokenResponse
                ) ?? throw new YoutubeExplodeException("Token is null");

            if (token.Error is not null)
            {
                if (token.Error == "authorization_pending")
                {
                    await Task.Delay(codeResponse.Interval * 1000);
                    continue;
                }
                else if (token.Error == "expired_token")
                {
                    Console.WriteLine("The device code has expired, restarting authorization flow");
                    return await AuthorizeAsync();
                }
                else
                {
                    throw new Exception($"Unhandled OAuth2 Error: {token.Error}");
                }
            }

            Console.WriteLine("Authorization successful");
            return new TokenData
            {
                AccessToken = token.AccessToken,
                Expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + token.ExpiresIn,
                RefreshToken = token.RefreshToken,
                TokenType = token.TokenType,
            };
        }
    }

    private Task<TokenData?> GetTokenAsync()
    {
        // Placeholder for actual token loading logic (e.g., from cache or file).
        return Task.FromResult<TokenData?>(null);
    }

    private void StoreToken(TokenData tokenData)
    {
        // Placeholder for actual token storing logic (e.g., in cache or file).
    }
}

// Classes to map API responses
internal class TokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public required long ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; set; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; set; }

    [JsonPropertyName("error")]
    public required string Error { get; set; }
}

internal class DeviceCodeResponse
{
    [JsonPropertyName("device_code")]
    public required string DeviceCode { get; set; }

    [JsonPropertyName("user_code")]
    public required string UserCode { get; set; }

    [JsonPropertyName("verification_url")]
    public required string VerificationUrl { get; set; }

    [JsonPropertyName("interval")]
    public required int Interval { get; set; }
}

/// <summary>
/// Token data from oauth login
/// </summary>
public class TokenData
{
    /// <summary>
    /// Access token
    /// </summary>
    public required string AccessToken { get; set; }

    /// <summary>
    /// Expire time in miliseconds
    /// </summary>
    public required long Expires { get; set; }

    /// <summary>
    /// Token to refresh access token
    /// </summary>
    public required string RefreshToken { get; set; }

    /// <summary>
    /// Type of token
    /// </summary>
    public required string TokenType { get; set; }
}
