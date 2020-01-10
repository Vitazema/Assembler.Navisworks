using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Server.Lib.Ex
{
  public static class RevitServer_old
  {
    public static bool CanBeParsedToRsnPath(this string path)
    {
      return (path.StartsWith("\\\\") || path.StartsWith(@"RSN:\\"))
        && path.EndsWith(".rvt");
    }
    public static bool IsValidPathForRsn(this string path)
    {
      return Regex.IsMatch(path, @"(?!(.*\\(Projects|Prg|Prj)\\.*))^RSN:\\\\.*\\\d{4}.*\.rvt$", RegexOptions.Compiled);
    }
    public static bool IsRvtRsnFile(this string path)
    {
      return path.EndsWith(".rvt");
    }
    public static bool IsValidRsnStringPath(this string path)
    {
      return path.StartsWith(@"RSN:\\");
    }
    public static IEnumerable<DirectoryInfo> GetFolderDirectories(string rootDir, int depth = 0)
    {
      return GetFolderDirectories(new DirectoryInfo(rootDir), depth);
    }
    public static IEnumerable<DirectoryInfo> GetFolderDirectories(DirectoryInfo rootDir, int depth = 0)
    {
      yield return rootDir;
      if (depth != 0)
      {
        if (rootDir.Name.EndsWith(".rvt"))
        {
          yield break;
        }
        foreach (var dir in rootDir.EnumerateDirectories())
        {
          foreach (var subDir in GetFolderDirectories(dir, depth - 1))
          {
            if (subDir.Name.EndsWith(".rvt"))
              yield return subDir;
          }
        }
      }
    }

    /// <summary>
    /// Убирает четыре знака в расширение файла, если таковое имеется. Работает с .dwg или .rvt
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string RemoveFileExtension(this string fileName)
    {
      return fileName.EndsWith(".rvt")
             || fileName.EndsWith(".dwg")
        ? fileName.Remove(fileName.Length - 4) : fileName;
    }

  }
}
