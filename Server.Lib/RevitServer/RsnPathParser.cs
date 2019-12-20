using Server.Lib.RevitServer;

namespace Revit.Tools.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text.RegularExpressions;
  using Autodesk.Revit.DB;
  using Microsoft.WindowsAPICodePack.Dialogs;

  public static class RsnPathParser
  {
    public static string ConvertNetworkToRsnPath(this string path)
    {
      if (path.CanBeParsedToRsnPath())
      {
        var splittedPath = path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

        if (splittedPath[0] == "RSN:")
        {
          splittedPath = splittedPath.Skip(1).ToArray();
        }

        var server = splittedPath[0];
        server = server.Replace(".main.picompany.ru", "");

        int projectSiteIndex = splittedPath.ToList().FindIndex(p => Regex.IsMatch(p, @"^\d\d\d\d"));

        var projectPath = splittedPath.Skip(projectSiteIndex).ToList();

        var modelFullName = projectPath.Last();

        projectPath.RemoveAt(projectPath.Count() - 1);

        var compiledPath = "RSN:\\\\" + server + "\\" + string.Join("\\", projectPath.ToList()) + "\\" + modelFullName;

        if (compiledPath.IsValidPathForRsn())
          return compiledPath;
        else
        {
          throw new ArgumentException("Путь не пригоден для создания ссылки на модель RSN");
        }
      }
      else
      {
        return null;
      }

    }
    public static List<ModelPath> GetModelsByFolderPath(DirectoryInfo path)
    {
      var modelPaths = new List<ModelPath>();

      // Be cautious when add english letters standalone
      var excludeFolders = new string[]
      {
        "ДДУ",
        "РМП",
        "восстановление",
        "старый",
        "архив",
        "lib"
      };
      var pattern = $"({string.Join("|", excludeFolders)})";
      // change to .contains() with lowering
      var revitFiles = RevitServer.GetFolderDirectories(path, 2)
        .Where(d => !Regex.IsMatch(d.FullName, pattern, RegexOptions.IgnoreCase))
        .ToList();
      if (revitFiles.Count != 0)
      {
        revitFiles.RemoveAt(0);

        foreach (var filePathInfo in revitFiles)
        {
          var filePath = filePathInfo.FullName;
          var networkRsnPath = RsnHelper.ConvertFilePathToRsnPath(filePath);
          //var networkRsnPath = filePath.ConvertNetworkToRsnPath();
          if (networkRsnPath != null)          
            modelPaths.Add(ModelPathUtils.ConvertUserVisiblePathToModelPath(networkRsnPath));
        }
        return modelPaths;

      }
      else
      {
        return modelPaths;
      }
    }

    public static DirectoryInfo SelectFolder()
    {
      // Collect folders to update
      var dialog = new CommonOpenFileDialog();
      dialog.IsFolderPicker = true;
      CommonFileDialogResult result = dialog.ShowDialog();
      if (result.ToString() == "Ok")
      {
        var path = dialog.FileName;
        return new DirectoryInfo(path);
      }
      else
      {
        return null;
      }

    }
  }
}
