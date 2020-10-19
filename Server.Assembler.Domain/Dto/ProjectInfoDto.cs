namespace Server.Assembler.Domain.Dto
{
  /// <summary>
  ///   Информация о проекте
  /// </summary>
  public class ProjectInfoDto
  {
    /// <summary>
    ///   Имя проекта
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///   Адрес 1
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    ///   Адрес 2
    /// </summary>
    public string AddressLine2 { get; set; } = string.Empty;
  }
}