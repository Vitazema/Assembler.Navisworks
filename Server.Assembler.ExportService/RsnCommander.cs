using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Server.Assembler.ModelExportService
{
  public class RsnCommander
  {
    private readonly ILogger _logger;

    public RsnCommander(ILogger logger)
    {
      _logger = logger;
    }

    public string CreateLocalFile(ModelFile file, string rvtFileOutput)
    {
      try
      {
        // Configuration copy command
        // TODO: long callback if file doesn't exist on filepath

        if (!Directory.Exists(file.SysFilePath))
          throw new ArgumentException("Файл .rvt не найден на RSN");

        // TODO: inject loader .exe to this solution
        var loader = @"C:\Program Files\Autodesk\Revit 2019\RevitServerToolCommand\RevitServerTool.exe";

        if (!File.Exists(loader))
          throw new Exception(
            "Ревит не установлен на текущей машине, либо по пути невозможно определить имя сервера или версию файла\n" +
            $"{loader} - не существует");

        var loaderArgs = $"l \"{file.ProjectFileFullPathWithoutServername}\" " +
                         $"-d \"{rvtFileOutput}\" " +
                         $"-s \"{file.ServerName}\" " +
                         "-o";

        // Configuration and start copy process with logging
        var processInfo = new ProcessStartInfo(loader, loaderArgs)
        {
          CreateNoWindow = false,
          UseShellExecute = false,
          RedirectStandardError = true,
          RedirectStandardOutput = true
        };

        var process = Process.Start(processInfo);

        var processOut = process.StandardOutput
          .ReadToEnd()
          .Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries)
          .Skip(2)
          .ToArray();
        var log = string.Join("\n", processOut);

        // Check if file successfully copied from RSN
        if (!File.Exists(rvtFileOutput))
          throw new Exception($"Не удалось создать файл по пути: {rvtFileOutput}\nОшибка: {log}");

        file.RvtFilePath = rvtFileOutput;
        return log;
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message);
        return e.Message;
      }
    }
  }
}