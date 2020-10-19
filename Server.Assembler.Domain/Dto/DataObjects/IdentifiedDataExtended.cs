namespace Server.Assembler.Domain.Dto.DataObjects
{
  /// <summary>
  ///   Расширенные данные ответа
  /// </summary>
  public class IdentifiedDataExtended : IdentifiedData
  {
    /// <summary>
    ///   Атрибуты
    /// </summary>
    public VersionedAttributes Attributes { get; set; }
  }
}