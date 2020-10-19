using System;
using System.Net;
using System.Threading.Tasks;
using Autodesk.Forge;
using Microsoft.AspNetCore.Mvc;
using Server.Assembler.Domain.Entities;
using static System.Environment;

namespace Server.Assembler.Api.Controllers
{
  public class OAuthController : ControllerBase
  {
    [HttpGet]
    [Route("api/forge/oauth/token")]
    public async Task<AccessToken> GetPublicTokenAsync()
    {
      var creds = await Credentials.FromSessionAsync(Request.Cookies, Response.Cookies);

      if (creds == null)
      {
        Response.StatusCode = (int) HttpStatusCode.Unauthorized;
        return new AccessToken();
      }

      return new AccessToken
      {
        access_token = creds.TokenPublic,
        expires_in = (int) creds.ExpiresAt.Subtract(DateTime.Now).TotalSeconds
      };
    }

    [HttpGet]
    [Route("api/forge/oauth/signout")]
    public IActionResult Signout()
    {
      // finish session
      Credentials.Signout(Response.Cookies);

      return Redirect("/");
    }

    [HttpGet]
    [Route("api/forge/oauth/url")]
    public string GetOAuthURL()
    {
      // prepare the sign-in URL
      Scope[] scopes = {Scope.DataRead};
      var threeLeggedApi = new ThreeLeggedApi();
      var oauthUrl = threeLeggedApi.Authorize(
        GetEnvironmentVariable("FORGE_CLIENT_ID"),
        oAuthConstants.CODE,
        GetEnvironmentVariable("FORGE_CALLBACK_URL"),
        new[] {Scope.DataRead, Scope.DataCreate, Scope.DataWrite, Scope.ViewablesRead});

      return oauthUrl;
    }

    [HttpGet]
    [Route("api/forge/callback/oauth")]
    public async Task<IActionResult> OAuthCallbackAsync(string code)
    {
      if (string.IsNullOrWhiteSpace(code))
        return Redirect("/");
      // create credentials from oAuth CODE
      var creds = await Credentials.CreateFromCodeAsync(code, Response.Cookies);

      return Redirect("/");
    }

    [HttpGet]
    [Route("api/forge/clientid")]
    public dynamic GetClientID()
    {
      return new {id = GetEnvironmentVariable("FORGE_CLIENT_ID")};
    }
  }
}