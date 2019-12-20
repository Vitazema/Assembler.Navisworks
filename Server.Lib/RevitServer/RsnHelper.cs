using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Server.Lib.Extensions;

namespace Server.Lib.RevitServer
{
  public static class RsnHelper
  {
    public static readonly string[] possibleRsnProjectFolders = {"prj", "project", "projects", "prg"};

    public static Dictionary<int, List<string>> rsnServerList
    {
      get
      {
        using (var sr = new StreamReader(ExecutionUtils.GetAssemblyDirectory()
                                         + "\\RsnServers.json"))
        {
          var json = sr.ReadToEnd();
          return JsonConvert.DeserializeObject<RsnServers>(json).RsnServerList;
        }
      }
    }

    /// <summary>
    ///   Convert any valid file path to equvalent ModelPath.
    ///   Path must have 4 digital project tag at the beginning.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>File name with any type condition</returns>
    public static string ConvertFilePathToRsnPath(this string path)
    {
      return "RSN:\\\\" + path.ExtractServerNameWithoutDomain() + "\\" + path.ExtractProjectFilePath();
    }

    public static string ExtractServerNameWithoutDomain(this string path)
    {
      // todo: check possible overtype json query error with \\ or \
      var splittedPath = path.Split(new[] {'\\'}, StringSplitOptions.RemoveEmptyEntries);

      if (splittedPath[0] == "RSN:") splittedPath = splittedPath.Skip(1).ToArray();

      if (splittedPath[0].Contains('.')) return splittedPath[0].ToLower().Split('.').FirstOrDefault();

      return splittedPath.FirstOrDefault();
    }

    public static string ExtractProjectFilePath(this string path)
    {
      var splittedPath = path
        .Split(new[] {'\\'}, StringSplitOptions.RemoveEmptyEntries);


      var projectSiteIndex = splittedPath.ToList().FindLastIndex(p =>
        p.ToLower()
          .ContainsAny(possibleRsnProjectFolders.Concat(rsnServerList.SelectMany(s => s.Value)).ToArray()));

      return string.Join("\\", splittedPath.Skip(projectSiteIndex + 1));
    }

    public static bool IsValidForRsnModelPath(this string path)
    {
      var compiledOrProjectFolders = string.Join("|", possibleRsnProjectFolders);
      return Regex.IsMatch(path, $@"(?!(.*\\({compiledOrProjectFolders}|$)\\.*))^RSN:\\\\.*\.rvt$",
               RegexOptions.IgnoreCase) &&
             path.ContainsAny(rsnServerList.SelectMany(s => s.Value).ToArray());
    }

    public static List<ModelPath> GetModelsByFolderPath(DirectoryInfo path)
    {
      var modelPaths = new List<ModelPath>();

      // Be cautious when add english letters standalone
      var excludeFolders = new[]
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
      var revitFiles = GetFolderDirectories(path, 2)
        .Where(d => !Regex.IsMatch(d.FullName, pattern, RegexOptions.IgnoreCase))
        .ToList();
      if (revitFiles.Count != 0)
      {
        revitFiles.RemoveAt(0);

        foreach (var filePathInfo in revitFiles)
        {
          var filePath = filePathInfo.FullName;
          var networkRsnPath = ConvertFilePathToRsnPath(filePath);
          //var networkRsnPath = filePath.ConvertNetworkToRsnPath();
          if (networkRsnPath != null)
            modelPaths.Add(ModelPathUtils.ConvertUserVisiblePathToModelPath(networkRsnPath));
        }

        return modelPaths;
      }

      return modelPaths;
    }

    public static IEnumerable<DirectoryInfo> GetFolderDirectories(DirectoryInfo rootDir, int depth = 0)
    {
      yield return rootDir;
      if (depth != 0)
      {
        if (rootDir.Name.EndsWith(".rvt")) yield break;
        foreach (var dir in rootDir.EnumerateDirectories())
        foreach (var subDir in GetFolderDirectories(dir, depth - 1))
          if (subDir.Name.EndsWith(".rvt"))
            yield return subDir;
      }
    }
  }
}
