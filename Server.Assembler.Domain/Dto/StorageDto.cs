namespace Server.Assembler.Domain.Dto
{
  /// <summary>
  ///   Данные хранилища для загружаемого файла
  /// </summary>
  public class StorageDto
  {
    /// <summary>
    ///   Идентификатор хранилища
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///   Ключ хранилища
    /// </summary>
    public string BucketKey { get; set; } = string.Empty;

    /// <summary>
    ///   Имя загружаемого объекта
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///   Тип хранилища
    /// </summary>
    public string Type { get; set; } = string.Empty;
  }
}