namespace Server.Assembler.Domain.Dto
{
  /// <summary>
  ///   Информация о файле
  /// </summary>
  public class FileInfoDto
  {
    /// <summary>
    ///   Путь к загружаемому файлу
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///   Имя файла
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    ///   Название проекта для загрузки файла
    /// </summary>
    public string TargetProjectName { get; set; } = string.Empty;

    /// <summary>
    ///   Название папки для загрузки файла
    /// </summary>
    public string TargetFolderName { get; set; } = string.Empty;

    /// <summary>
    ///   Главная папка
    /// </summary>
    public MainFolder MainFolder { get; set; } = MainFolder.File;
  }
}