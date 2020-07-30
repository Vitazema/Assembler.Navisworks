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
    public string outPath { get; set; }

    public RsnFileInfo(string path, bool rsnStructure, string outFolder)
    {
      if (path.Length == 0)
        throw new Exception("Необходимо имя файла");

      rawFilePath = path;

      foreach (KeyValuePair<int, List<string>> keyValuePair in RsnHelper.rsnServerListFromConfigFile())
      {
        if (keyValuePair.Value.Contains(serverName))
        {
          serverVersion = keyValuePair.Key;
          break;
        }
      }

      //foreach (KeyValuePair<int, List<string>> rsnServerList in RsnHelper.RsnServerListFromResources("RsnServers.json"))
      //{
      //  rsn
      //}
      //Regex.IsMatch(RsnHelper.rsn)
      if (serverVersion == 0)
      { 
        throw new ArgumentException("Имя сервера не определено, либо сервер неизвестен. Файл должен находиться на RSN");
      }

      fileFullName = Path.GetFileName(rawFilePath);
      projectDirectory = Path.GetDirectoryName(projectFileFullPathWithoutServername);

      if (outFolder != null)
      {
        outPath = Path.Combine(outFolder, rsnStructure ? projectFileFullPathWithoutServername : fileFullName);
      }
      else
      {
        outPath = Path.Combine(Path.GetTempPath(),
          serverName,
          rsnStructure ? projectFileFullPathWithoutServername : fileFullName);
      }
    }
  }
}
