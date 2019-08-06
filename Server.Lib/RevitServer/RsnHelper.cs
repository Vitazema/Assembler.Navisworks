using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Lib.Extensions;

namespace Server.Lib.RevitServer
{
  public static class RsnHelper
  {
    public static Dictionary<int, List<string>> rsnServerList
    {
      get
      {
        using (StreamReader sr = new StreamReader(ExecutionUtils.GetAssemblyDirectory()
                                                  + "\\RsnServers.json"))
        {
          string json = sr.ReadToEnd();
          return JsonConvert.DeserializeObject<RsnServers>(json).RsnServerList;
        }
      }
    }

    public static readonly string[] possibleRsnProjectFolders = {"prj", "project", "projects", "prg"};

    /// <summary>
    /// Convert any valid file path to equvalent ModelPath.
    /// Path must have 4 digital project tag at the beginning.
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
      var splittedPath = path.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

      if (splittedPath[0] == "RSN:")
      {
        splittedPath = splittedPath.Skip(1).ToArray();
      }

      if (splittedPath[0].Contains('.'))
      {
        return splittedPath[0].ToLower().Split('.').FirstOrDefault();
      }

      return splittedPath.FirstOrDefault();
    }

    public static string ExtractProjectFilePath(this string path)
    {
      var splittedPath = path
        .Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);


      int projectSiteIndex = splittedPath.ToList().FindLastIndex(p =>
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
  }
}
