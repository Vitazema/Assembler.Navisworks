using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Server.Assembler.Domain.Entities;

namespace Server.Assembler.ModelExportService
{
  public class RsnCommander
  {
    private readonly ILogger _logger;

    public RsnCommander(ILogger logger)
    {
      _logger = logger;
    }
    public static string CreateLocalFile(RsnFileInfo file)
    {
      try
      {
        // Configuration copy command
        // TODO: long callback if file doesn't exist on filepath

        if (!Directory.Exists(file.rawFilePath))
          throw new ArgumentException("File doesn't exists in RSN");

        var loader = $@"C:\Program Files\Autodesk\Revit {file.serverVersion.ToString()}\RevitServerToolCommand\RevitServerTool.exe";

        if(!File.Exists(loader))
          throw new Exception("Ревит не установлен на текущей машине, либо по пути невозможно определить имя сервера или версию файла\n" +
                              $"{loader} - не существует");;       

        var loaderArgs = "createLocalRvt " +
                         "\"" + file.projectFileFullPathWithoutServername + "\"" +
                         " -destination " +
                         "\"" + file.tempPath + "\"" +
                         $" -s {file.serverName} -o";

        // Configuration and start copy process
        var processInfo = new ProcessStartInfo(loader, loaderArgs)
        {
          CreateNoWindow = false,
          UseShellExecute = false,
          RedirectStandardError = true,
          RedirectStandardOutput = true
        };

        var process = Process.Start(processInfo);

        var output = process.StandardOutput
          .ReadToEnd()
          .Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries)
          .Skip(2)
          .ToArray();

        if (output.Length > 0)
          return string.Join("\n", output);

        return process.StandardOutput.ReadToEnd() + "\n" +
               process.StandardError.ReadToEnd();

        // Validate file creation
        //if (!File.Exists())
      }
      catch (Exception e)
      {
        //_logger.LogError(e.Message);
        return e.Message;
      }
    }
  }
}
