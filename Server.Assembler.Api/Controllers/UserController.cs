using System.Threading.Tasks;
using Autodesk.Forge;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Server.Assembler.Api.Controllers
{
  public class UserController : ControllerBase
  {
    [HttpGet]
    [Route("api/forge/user/profile")]
    public async Task<JObject> GetUserProfileAsync()
    {
      var creds = await Credentials.FromSessionAsync(Request.Cookies, Response.Cookies);
      if (creds == null)
        return null;

      var userApi = new UserProfileApi();
      userApi.Configuration.AccessToken = creds.TokenInternal;

      // get the user profile
      var userProfile = await userApi.GetUserProfileAsync();

      // prepare a response with name & picture
      dynamic response = new JObject();
      response.name = $"{userProfile.firstName} {userProfile.lastName}";
      response.picture = userProfile.profileImages.sizeX40;
      return response;
    }
  }
}