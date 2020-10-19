using System;
using System.Collections.Generic;
using System.IO;
using Revit.Domain.RevitServer;

namespace Server.Assembler.ModelExportService
{
  public class ModelFile
  {
    /// <summary>
    ///   Create .rvt file info. For some reason, it throws an exception in runtime. It define as normal behaivour fow now.
    /// </summary>
    /// <param name="sysFilePath">Full system path to a file.</param>
    public ModelFile(string sysFilePath)
    {
      SysFilePath = sysFilePath ?? throw new ArgumentNullException(nameof(sysFilePath));
    }

    /// <summary>
    ///   Server's path to RSN model folder.
    /// </summary>
    public string SysFilePath { get; }

    public string FullFileName => Path.GetFileName(SysFilePath);

    public int RevitVersion { get; set; }

    /// <summary>
    ///   Project directory.
    /// </summary>
    public string ProjectFileFullPathWithoutServername { get; set; }

    /// <summary>
    ///   RSN server name.
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///   RSN project directory.
    /// </summary>
    public string ProjectDirectory { get; set; }

    /// <summary>
    ///   Local .rvt file location.
    /// </summary>
    public string RvtFilePath { get; set; }

    /// <summary>
    ///   RSN visible path to file.
    /// </summary>
    public string RsnFilePath
    {
      get
      {
        var parsedName = SysFilePath.ConvertFilePathToRsnPath();
        if (!parsedName.IsValidForRsnModelPath())
          throw new Exception("Путь к файлу проекта не является правильным для RSN");
        return parsedName;
      }
    }

    public void TryValidate()
    {
      if (SysFilePath.Length == 0)
        throw new Exception("Необходимо назначить путь к файлу");

      if (Directory.Exists(SysFilePath))
      {
        ServerName = SysFilePath.ExtractServerNameWithoutDomain();
        ProjectFileFullPathWithoutServername = SysFilePath.ExtractProjectFilePath();
        ProjectDirectory = Path.GetDirectoryName(ProjectFileFullPathWithoutServername);

        // Define Revit Server version

        //foreach (KeyValuePair<int, List<string>> rsnServerList in RsnHelper.RsnServerListFromResources("RsnServers.json"))
        //{
        //  rsn
        //}
        //Regex.IsMatch(RsnHelper.rsn)
        foreach (KeyValuePair<int, List<string>> keyValuePair in RsnHelper.rsnServerListFromConfigFile())
          if (keyValuePair.Value.Contains(ServerName))
          {
            RevitVersion = keyValuePair.Key;
            break;
          }

        if (RevitVersion == 0)
          throw new ArgumentException(
            "Имя сервера не определено, либо сервер неизвестен. Файл должен находиться на RSN");
      }
      else if (File.Exists(SysFilePath))
      {
        RvtFilePath = SysFilePath;
        // TODO: define revit file version
      }
      else
      {
        throw new Exception($"Revit source: {SysFilePath} doesn't exist.");
      }
    }
  }
}