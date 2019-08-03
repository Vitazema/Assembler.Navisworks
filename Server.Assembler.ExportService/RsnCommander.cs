using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Server.Assembler.Domain.Entities;

namespace Server.Assembler.ModelExportService
{
  public class RsnCommander
  {
    public string CreateLocalFile(RsnFileInfo file)
    {
      try
      {
        // Configuration copy command
        // TODO: long callback if file doesn't exist on filepath
        var loader = $@"C:\Program Files\Autodesk\Revit {file.serverVersion.ToString()}\RevitServerToolCommand\RevitServerTool.exe";

        if(!File.Exists(loader))
          return "Ревит не установлен на текущей машине, либо по пути невозможно определить имя сервера или версию файла\n" +
            $"Инфо: {loader}";

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
      }
      catch (Exception e)
      {
        return e.Message;
      }
    }
  }
}
