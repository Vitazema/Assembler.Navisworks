namespace Server.Assembler.Domain.Dto
{
  /// <summary>
  ///   Информация о папке
  /// </summary>
  public class FolderInfoDto
  {
    /// <summary>
    ///   Имя проекта
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    ///   Имя родительской папки
    /// </summary>
    public string ParentFolderName { get; set; } = string.Empty;

    /// <summary>
    ///   Имя папки
    /// </summary>
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    ///   Главная папка
    /// </summary>
    public MainFolder MainFolder { get; set; } = MainFolder.File;
  }
}