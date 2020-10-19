using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Server.Assembler.Domain.Dto;

namespace Bim360.Assembler.Services
{
  public class AuthService
  {
    private const string Login = "sys.bim";
    private const string Password = "kgf!Dsd5";

    private const string ClientId = "wc66wOhGgDAG3hwj8KljsXR0Xycg6oEM";
    private const string ClientSecret = "90VHOvZIvqAmmqve";
    private const string GrantType = "client_credentials";
    private const string Scope = "data:create data:read data:write account:write";

    private const string AccessToken = "access_token";
    private const string AuthPath = "authentication/v1/authenticate";
    private const string FormHeader = "application/x-www-form-urlencoded";

    /// <summary>
    ///   Возвращает данные авторизации
    /// </summary>
    /// <returns>Данные авторизации</returns>
    public static NetworkCredential GetNetworkCredential()
    {
      return new NetworkCredential(Login, Password);
    }

    /// <summary>
    ///   Возвращает токен доступа к API
    /// </summary>
    /// <returns>Токен доступа к API</returns>
    public async Task<string> GetAccessToken()
    {
      try
      {
        var client = new HttpClient {BaseAddress = new Uri(Constants.ApiUrl)};
        var request = new HttpRequestMessage(HttpMethod.Post, AuthPath)
        {
          Content = new FormUrlEncodedContent(new Dictionary<string, string>
          {
            {"client_id", ClientId},
            {"client_secret", ClientSecret},
            {"grant_type", GrantType},
            {"scope", Scope}
          })
        };
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(FormHeader);

        var response = await client.SendAsync(request);
        //await response.HandleErrorAsync();
        var content = await response.Content.ReadAsStringAsync();

        return JObject.Parse(content)[AccessToken].ToString();
      }
      catch (Exception ex)
      {
        throw new Exception(
          $"Не удалось получить \"{AccessToken}\". {ex.Message} {ex.InnerException?.Message} error code: 400");
      }
    }
  }
}