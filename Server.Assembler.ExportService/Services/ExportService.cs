using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Assembler.Domain;
using Server.Assembler.Domain.Entities;
using Server.Lib.Documents;

namespace Server.Assembler.ModelExportService.Services
{
  public class ExportService : IExportService
  {
    ILogger logger;
    public int maxThreads { get; set; } = Environment.ProcessorCount;

    public const string defaultExportFolder = @"\\picompany.ru\pikp\NAVIS-EXP\_Экспорт";

    private readonly NavisCommander navisCommander;
    private readonly RsnCommander rsnCommander;

    public ExportService(ILogger<ExportService> logger, IOptions<Perfomance> config)
    {
      this.logger = logger;
      navisCommander = new NavisCommander();
      if (!config.Value.AutoThreads)
        maxThreads = config.Value.MaxDegreeOfParallelism;
    }

    //// TODO: check if can catch configuration in runtime
    //// when service is called
    //public ExportService(IOptions<Perfomance> config) : this()
    //{
    //}

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
          {
            var ex = new Exception("Файл не удалось создать во временной папке");
            logger.LogError(ex, ex.Message);
            throw ex;
          }

          using (var sw = new StreamWriter(File.Create(tempConfigFile), Encoding.UTF8))
          {
            sw.WriteLine(file.tempPath);
          }

          var outputDirectory = outFolder;
          if (rsnStructure)
            outputDirectory = Path.Combine(outFolder, file.projectDirectory);
          var navisJobOutput =
            navisCommander.BatchExportToNavis(tempConfigFile, file.serverVersion, false, outputDirectory);
          logger.LogInformation(navisJobOutput, file);
          log.Add(navisJobOutput);
        }
        catch (Exception e)
        {
          logger.LogError(e, e.Message);
          log.Add(e.Message);
        }
      });

      return string.Join("\n", log);
    }
    public string BatchParralelExportModelsTolFolder(List<RsnFileInfo> files, bool rsnStructure, string outFolder = defaultExportFolder)
    {
      var options = new ParallelOptions() { MaxDegreeOfParallelism = maxThreads };
      // export logs to parallel dump
      var log = new ConcurrentBag<string>();
      Parallel.ForEach(files, options, file =>
      {
        try
        {
          var rsnTempFile = Path.GetTempPath() + "temp" + Guid.NewGuid() + ".txt";

          // Check if file successfuly copied from RSN,
          // then start copy to destination
          if (!File.Exists(file.tempPath))
          {
            var ex = new Exception("Файл не удалось создать во временной папке");
            logger.LogError(ex, ex.Message);
            throw ex;
          }

          using (var sw = new StreamWriter(File.Create(rsnTempFile), Encoding.UTF8))
          {
            sw.WriteLine(file.tempPath);
          }

          var outputDirectory = outFolder;
          if (rsnStructure)
            outputDirectory = Path.Combine(outFolder, file.projectDirectory);
          var rsnFileExportLog = rsnCommander.CreateLocalFile(file);

        }
        catch (Exception e)
        {
          logger.LogError(e, e.Message);
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
