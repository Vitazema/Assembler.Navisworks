using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Server.Lib.RevitServer;

namespace Server.Assembler.Domain.Entities
{
  public class RsnFileInfo
  {
    public string rsnFilePath
    {
      get
      {
        var parsedName = rawFilePath.ConvertFilePathToRsnPath();
        if (!parsedName.IsValidForRsnModelPath())
          throw new Exception("Путь к файлу проекта не является правильным для RSN");
        return parsedName;
      }
    }

    public string rawFilePath { get; set; }

    public string fileFullName { get; set; }

    public int serverVersion { get; set; }

    public string projectFileFullPathWithoutServername => rawFilePath.ExtractProjectFilePath();

    public string serverName => rawFilePath.ExtractServerNameWithoutDomain();

    public string projectDirectory { get; set; }

    /// <summary>
    /// Temporary server place for copied file from RSN (local user temp folder)
    /// </summary>
    public string tempPath { get; set; }

    public RsnFileInfo(string path)
    {
      if (path.Length == 0)
        throw new Exception("Необходимо имя файла");

      rawFilePath = path;

      foreach (KeyValuePair<int, List<string>> keyValuePair in RsnHelper.rsnServerList)
      {
        if (keyValuePair.Value.Contains(serverName))
        {
          serverVersion = keyValuePair.Key;
          break;
        }
      }

      if (serverVersion == 0)
      { 
        throw new ArgumentException("Имя сервера не определено, либо сервер неизвестен. Файл должен находиться на RSN");
      }

      fileFullName = Path.GetFileName(rawFilePath);
      
      tempPath = Path.Combine(Path.GetTempPath() + $"{serverName}\\{projectFileFullPathWithoutServername}");

      projectDirectory = Path.GetDirectoryName(projectFileFullPathWithoutServername);
    }
  }
}
