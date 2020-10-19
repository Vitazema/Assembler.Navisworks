namespace Server.Assembler.Domain.Dto
{
  /// <summary>
  ///   Представляет элемент в системе Document Management
  /// </summary>
  public class ItemDto
  {
    /// <summary>
    ///   Тип элемента
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///   Версия
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    ///   Идентификатор элемента
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///   Имя элемента
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///   Отображаемое имя элемента
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
  }
}