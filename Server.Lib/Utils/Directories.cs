using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Server.Lib.Utils
{
  public static class Directories
  {
    public static void RemoveDirectories(params string[] args)
    {
      foreach (var directory in args)
        if (Directory.Exists(directory))
          try
          {
            Directory.Delete(directory, true);
            //log.Add($"Папка {directory} удалена");
          }
          catch (Exception ex)
          {
            //log.Add($"Ошибка при удалении папки {directory}:\n{ex.Message}");
            Debug.Write(ex.Message);
          }
        else
          Debug.Write("Папка не была удалена");
    }

    public static async Task<bool> RemoveDirectoriesAsync(params string[] args)
    {
      return await Task.Run(() =>
      {
        RemoveDirectories(args);
        return true;
      });
    }

    public static void CopyWholeDirectory(string sourcePath, string outputPath)
    {
      //Copy all directories & Replaces them with the same name
      foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        Directory.CreateDirectory(dirPath.Replace(sourcePath, outputPath));

      //Copy all files and replace any with the same name
      foreach (var existFilePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
      {
        var destFilePath = existFilePath.Replace(sourcePath, outputPath);

        // check if file cannot be overwritten due to read-only flag
        var fileInfo = new FileInfo(destFilePath);
        if (fileInfo.Exists)
          fileInfo.IsReadOnly = false;

        File.Copy(existFilePath, destFilePath, true);
      }
    }
  }
}
