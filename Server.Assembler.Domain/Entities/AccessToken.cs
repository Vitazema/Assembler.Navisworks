namespace Server.Assembler.Domain.Entities
{
  /// <summary>
  ///   Response for GetPublicToken
  /// </summary>
  public struct AccessToken
  {
    public string access_token { get; set; }
    public int expires_in { get; set; }
  }
}