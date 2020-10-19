namespace Server.Assembler.Domain.Dto.DataObjects
{
  /// <summary>
  ///   Атрибуты
  /// </summary>
  public class VersionedAttributes
  {
    /// <summary>
    ///   Имя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///   Отображаемое имя
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///   Расширение
    /// </summary>
    public Extension Extension { get; set; }
  }
}