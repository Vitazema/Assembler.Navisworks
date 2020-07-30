using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using Server.Lib.Ex;
using Server.Lib.Extensions;
using Server.Lib.Utils;

namespace Server.Lib.RevitServer
{
  public static class RsnHelper
  {
    public static readonly string[] possibleRsnProjectFolders = { "prj", "project", "projects", "prg" };

    public static Dictionary<int, List<string>> rsnServerListFromConfigFile()
    {
      var jsonPath = AssemblyUtils.GetAssemblyDirectory() + "\\RsnServers.json";
      if (!File.Exists(jsonPath))
        return null;
      using (var sr = new StreamReader(jsonPath))
      {
        var json = sr.ReadToEnd();
        return JsonConvert.DeserializeObject<RsnServers>(json).RsnServerList;
      }
    }

    public static Dictionary<int, List<string>> RsnServerListFromResources(string resourceName)
    {
      var assembly = Assembly.GetExecutingAssembly();
      var asll = assembly.GetManifestResourceNames();
      var embeddedResourcePathName = $"{assembly.GetName().Name}.{resourceName}";
      using (Stream resourceStream = assembly.GetManifestResourceStream(embeddedResourcePathName))
      {
        if (resourceStream == null)
          return null;
        using (var sr = new StreamReader(resourceStream))
        {
          var json = sr.ReadToEnd();
          return JsonConvert.DeserializeObject<RsnServers>(json).RsnServerList;
        }
      }
    }

    //public static int GetRsnFileVersion

    /// <summary>
    ///   Convert any valid file directory to equvalent ModelPath.
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
      var splittedPath = path.ToLower().Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

      if (splittedPath[0] == "RSN:") splittedPath = splittedPath.Skip(1).ToArray();

      var serverName = splittedPath.FirstOrDefault();

      // check if server contains domain name
      var domainName = Regex.Match(serverName, @"\.[a-zA-Z]+");
      if (domainName.Success){
        //splittedPath[0].Contains('.'))
        serverName = serverName.Substring(0, serverName.LastIndexOf(domainName.Value));
      }

      return serverName;
    }

    public static string ExtractProjectFilePath(this string path)
    {
      var splittedPath = path
        .Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

      var rsnServers = rsnServerListFromConfigFile()??RsnServerListFromResources("RsnServers.json");
      if (rsnServers == null)
        throw new Exception("Cannot find Revit server config file");
      var projectSiteIndex = splittedPath.ToList().FindLastIndex(p =>
        p.ToLower()
          .ContainsAny(possibleRsnProjectFolders.Concat(rsnServers.SelectMany(s => s.Value)).ToArray()));

      return string.Join("\\", splittedPath.Skip(projectSiteIndex + 1));
    }

    public static bool IsValidForRsnModelPath(this string path)
    {
      var compiledOrProjectFolders = string.Join("|", possibleRsnProjectFolders);
      return Regex.IsMatch(path, $@"(?!(.*\\({compiledOrProjectFolders}|$)\\.*))^RSN:\\\\.*\.rvt$",
               RegexOptions.IgnoreCase) &&
             path.ContainsAny(rsnServerListFromConfigFile().SelectMany(s => s.Value).ToArray());
    }

    public static List<ModelPath> GetModelsByFolder(DirectoryInfo directory)
    {
      var modelPaths = new List<ModelPath>();

      // Be cautious when add english letters standalone
      //var excludeFolders = new[]
      //{
      //  "ДДУ",
      //  "РМП",
      //  "восстановление",
      //  "старый",
      //  "архив",
      //  "lib"
      //};
      //var pattern = $"({string.Join("|", excludeFolders)})";
      // change to .contains() with lowering
      var revitFiles = GetFolderDirectories(directory, 2)
        //.Where(d => !Regex.IsMatch(d.FullName, pattern, RegexOptions.IgnoreCase))
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

    public static List<string> GetModelPathsByFolder(DirectoryInfo directory)
    {
      var fileOutput = new List<string>();
      var revitFiles = GetFolderDirectories(directory, 2).ToList();
      if (revitFiles.Count != 0)
      {
        revitFiles.RemoveAt(0);
        foreach (var filePathInfo in revitFiles)
        {
          fileOutput.Add(filePathInfo.FullName);
        }
      }
      return fileOutput;
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
    public static string ConvertNetworkToRsnPath_Old(this string path)
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
  }
}
