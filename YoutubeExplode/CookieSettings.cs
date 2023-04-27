namespace YoutubeExplode;

/// <summary>
///     Cookies used to authenticate requests.
/// </summary>
public class CookiesSettings
{
    /// <summary>
    ///     Cookies used to authenticate requests.
    /// </summary>
    /// <param name="sapisid"></param>
    /// <param name="psid"></param>
    public CookiesSettings(string sapisid, string psid)
    {
        Sapisid = sapisid;
        Psid = psid;
    }

    /// <summary>
    ///     Gets or sets the cookies.
    /// </summary>
    public string Sapisid { get; set; }

    /// <summary>
    ///     Gets or sets the cookies.
    /// </summary>
    public string Psid { get; set; }
}