using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Server.Assembler.Domain.Entities;

namespace Server.Assembler.ModelExportService.Services
{
  public class ExportService : IExportService
  {
    public const string exportFolder = @"\\picompany.ru\pikp\Dep\IT\_SR_Public\01_BIM\20_Выгрузки\_Tests";

    private readonly RsnCommander rsnCommander;

    private readonly NavisCommander navisCommander;

    public ExportService()
    {
      rsnCommander = new RsnCommander();
      navisCommander = new NavisCommander();
    }

    public string ExportModelToNavis(RsnFileInfo file)
    {
      var log = new List<string>();
      log.Add(rsnCommander.CreateLocalFile(file));

      var random = new Random();
      var tempConfigFile = Path.GetTempPath() + "temp" + random.Next(10000, 99999) + ".txt";
      using (var fs = File.Create(tempConfigFile))
      {
        byte[] line = new UTF8Encoding(true).GetBytes(file.tempPath);

        fs.Write(line, 0, line.Length);
      }

      var navisOutput = navisCommander.BatchExportToNavis(tempConfigFile, file.serverVersion, false, exportFolder);

      log.Add(navisOutput);

      return string.Join("\n", log);
    }

    public string BatchExportModelsToNavis(List<RsnFileInfo> files, string outFolder = exportFolder)
    {
      var log = new List<string>();

      var exportModels = files.GroupBy(x => x.serverVersion).ToDictionary(g => g.Key, g => g.ToList());

      foreach (KeyValuePair<int, List<RsnFileInfo>> group in exportModels)
      {
        var random = new Random();
        var tempConfigFile = Path.GetTempPath() + "temp" + random.Next(10000, 99999) + ".txt";

        using (var sw = new StreamWriter(File.Create(tempConfigFile), Encoding.UTF8))
        {
          foreach (var file in group.Value)
          {
            var fileLog = rsnCommander.CreateLocalFile(file);
            log.Add(fileLog);
            if (File.Exists(file.tempPath))
              sw.WriteLine(file.tempPath);
          }
        }
        var navisOutput = navisCommander.BatchExportToNavis(tempConfigFile, group.Key, false, outFolder);
        log.Add(navisOutput);
      }
      return string.Join("\n", log);
    }

    public string BatchModelExport(List<RsnFileInfo> files)
    {
      var log = new List<string>();
      foreach (var file in files)
      {
        try
        {
          log.Add(rsnCommander.CreateLocalFile(file));

          if (File.Exists(file.tempPath))
          {
            var destFilePath = Path.Combine(exportFolder, file.projectFileFullPathWithoutServername);
            File.Copy(file.tempPath, destFilePath, true);
            log.Add($"Model copied: {destFilePath}");
          }
          else
          {
            log.Add($"file creation failed: {file.tempPath}");
          }
        }
        catch (Exception e)
        {
          log.Add(e.Message);
        }
      }

      return string.Join("\n", log);
    }

    public string RevitModelExport(RsnFileInfo file, string folder = exportFolder)
    {
      var log = new List<string>();

      log.Add(rsnCommander.CreateLocalFile(file));

      // Check if file successfuly copied from RSN,
      // then start copy to destination
      if (File.Exists(file.tempPath))
      {
        var destFilePath = Path.Combine(folder, "Координация", file.fileFullName);
        File.Copy(file.tempPath, destFilePath, true);
        log.Add($"Модель скопирована: {destFilePath}");
      }
      else
      {
        log.Add($"Не удалось создать файл: {file.tempPath}");
      }

      return string.Join("\n", log);
    }
  }
}
