using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Server.Assembler.Domain;
using Server.Assembler.Domain.Entities;

namespace Server.Assembler.ModelExportService.Services
{
  public class ExportService : IExportService
  {
    public int maxThreads { get; set; } = 8;

    public const string defaultExportFolder = @"\\picompany.ru\pikp\Dep\IT\_SR_Public\01_BIM\20_Выгрузки";

    private readonly RsnCommander rsnCommander;

    private readonly NavisCommander navisCommander;

    public ExportService(IOptions<Perfomance> config)
    {
      rsnCommander = new RsnCommander();
      navisCommander = new NavisCommander();

      maxThreads = config.Value.MaxDegreeOfParallelism;
    }

    public string BatchParallelExportModelsToNavis(List<RsnFileInfo> files, bool rsnStructure, string outFolder = defaultExportFolder)
    {
      var options = new ParallelOptions() {MaxDegreeOfParallelism = maxThreads};
      var log = new ConcurrentBag<string>();
      Parallel.ForEach(files, options, file =>
      {
        try
        {
          var tempConfigFile = Path.GetTempPath() + "temp" + new Random().Next(1000000, 9999999) + ".txt";
          log.Add(rsnCommander.CreateLocalFile(file));

          // Check if file successfuly copied from RSN,
          // then start copy to destination
          if (!File.Exists(file.tempPath))
            throw new Exception("Файл не удалось создать во временной папке");

          using (var sw = new StreamWriter(File.Create(tempConfigFile), Encoding.UTF8))
          {
            sw.WriteLine(file.tempPath);
          }

          var outputDirectory = outFolder;
          if (rsnStructure)
            outputDirectory = Path.Combine(outFolder, file.projectDirectory);
          var navisJobOutput =
            navisCommander.BatchExportToNavis(tempConfigFile, file.serverVersion, false, outputDirectory);
          log.Add(navisJobOutput);
        }
        catch (Exception e)
        {
          log.Add(e.Message);
        }

      });

      return string.Join("\n", log);
    }

    public string BatchExportModelsToFolder(List<RsnFileInfo> files, bool rsnStructure, string outFolder = defaultExportFolder)
    {
      var log = new List<string>();

      var exportModels = files.GroupBy(x => x.serverVersion).ToDictionary(g => g.Key, g => g.ToList());

      foreach (KeyValuePair<int, List<RsnFileInfo>> group in exportModels)
      {
        foreach (var file in group.Value)
        {
          var fileLog = rsnCommander.CreateLocalFile(file);
          log.Add(fileLog);
        }
        
      }
      return string.Join("\n", log);
    }
  }
}
