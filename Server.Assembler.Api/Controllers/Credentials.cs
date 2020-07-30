using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Autodesk.Forge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using static System.Environment;

namespace Server.Assembler.Api.Controllers
{
  /// <summary>
  /// Store data in session
  /// </summary>
  public class Credentials
  {
    private const string FORGE_COOKIE = "ForgeApi";
    private Credentials() { }

    public string TokenInternal { get; set; }
    public string TokenPublic { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    public static async Task<Credentials> CreateFromCodeAsync(string code, IResponseCookies cookies)
    {
      var oauth = new ThreeLeggedApi();
      dynamic credentialInternal = await oauth.GettokenAsync(
        GetEnvironmentVariable("FORGE_CLIENT_ID"),
        GetEnvironmentVariable("FORGE_CLIENT_SECRET"),
        oAuthConstants.AUTHORIZATION_CODE,
        code,
        GetEnvironmentVariable("FORGE_CALLBACK_URL"));

      dynamic credentialPublic = await oauth.RefreshtokenAsync(
        GetEnvironmentVariable("FORGE_CLIENT_ID"),
        GetEnvironmentVariable("FORGE_CLIENT_SECRET"),
        "refresh_token",
        credentialInternal.refresh_token,
        new Scope[] { Scope.ViewablesRead });

      var creds = new Credentials
      {
        TokenInternal = credentialInternal.access_token,
        TokenPublic = credentialPublic.access_token,
        RefreshToken = credentialPublic.refresh_token,
        ExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in)
      };

      cookies.Append(FORGE_COOKIE, JsonConvert.SerializeObject(creds));

      return creds;
    }

    /// <summary>
    /// Restore the credentials from the session object, refresh if needed
    /// </summary>
    /// <param name="requestCookie"></param>
    /// <param name="responseCookie"></param>
    /// <returns></returns>
    public static async Task<Credentials> FromSessionAsync(IRequestCookieCollection requestCookie, IResponseCookies responseCookie)
    {
      if (requestCookie == null || !requestCookie.ContainsKey(FORGE_COOKIE)) return null;

      var creds = JsonConvert.DeserializeObject<Credentials>(requestCookie[FORGE_COOKIE]);

      if (creds.ExpiresAt < DateTime.Now)
      {
        await creds.RefreshAsync();
        responseCookie.Delete(FORGE_COOKIE);
        responseCookie.Append(FORGE_COOKIE, JsonConvert.SerializeObject(creds));
      }

      return creds;
    }

    /// <summary>
    /// Refresh an internal and external credentials
    /// </summary>
    /// <returns></returns>
    private async Task RefreshAsync()
    {
      var oauth = new ThreeLeggedApi();

      dynamic credentialInternal = await oauth.RefreshtokenAsync(
        GetEnvironmentVariable("FORGE_CLIENT_ID"),
        GetEnvironmentVariable("FORGE_CLIENT_SECRET"),
        "refresh_token",
        RefreshToken,
        new Scope[] { Scope.DataRead, Scope.DataCreate, Scope.DataWrite, Scope.ViewablesRead });

      dynamic credentialPublic = await oauth.RefreshtokenAsync(
        GetEnvironmentVariable("FORGE_CLIENT_ID"),
        GetEnvironmentVariable("FORGE_CLIENT_SECRET"),
        "refresh_token",
        credentialInternal.refresh_token,
        new Scope[] { Scope.ViewablesRead });

      TokenInternal = credentialInternal.acces_token;
      TokenPublic = credentialPublic.acces_token;
      RefreshToken = credentialPublic.refresh_token;
      ExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in);
    }

    public static void Signout(IResponseCookies cookies)
    {
      cookies.Delete(FORGE_COOKIE);
    }
  }
}
