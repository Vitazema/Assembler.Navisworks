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
    private readonly ILogger _logger;
    private readonly NavisCommander navisCommander;
    private readonly RsnCommander rsnCommander;

    public int maxThreads { get; set; } = Environment.ProcessorCount;

    public const string defaultNavisExportFolder = @"\\picompany.ru\pikp\NAVIS-EXP\_Экспорт";
    public string defaultRsnExportFolder;

    public ExportService(ILogger<ExportService> logger, IOptions<Perfomance> config)
    {
      _logger = logger;
      navisCommander = new NavisCommander();
      rsnCommander = new RsnCommander(logger);
      if (!config.Value.AutoThreads)
        maxThreads = config.Value.MaxDegreeOfParallelism;
      defaultRsnExportFolder = Path.Combine(Path.GetTempPath(), "RsnExport");
    }

    //// TODO: check if can catch configuration in runtime
    //// when service is called
    //public ExportService(IOptions<Performance> config) : this()
    //{
    //}

    public string ParallelExportModelsToNavis(ExportTask task) //List<RsnFileInfo> files, bool rsnStructure, string outFolder = defaultNavisExportFolder)
    {
      var options = new ParallelOptions() { MaxDegreeOfParallelism = maxThreads };
      var log = new ConcurrentBag<string>();
      Parallel.ForEach(task.Files, options, rawRsnFilePath =>
      {
        try
        {
          var fileInfo = new RsnFileInfo(rawRsnFilePath, task.RsnStructure, task.OutFolder);

          var tempConfigFile = Path.GetTempPath() + "temp" + new Random().Next(1000000, 9999999) + ".txt";
          log.Add(rsnCommander.CreateLocalFile(fileInfo));

          using (var sw = new StreamWriter(File.Create(tempConfigFile), Encoding.UTF8))
          {
            sw.WriteLine(fileInfo.outPath);
          }

          var outputDirectory = task.OutFolder;
          if (task.RsnStructure)
            outputDirectory = Path.Combine(task.OutFolder, fileInfo.projectDirectory);

          var navisJobOutput =
            navisCommander.BatchExportToNavis(tempConfigFile, fileInfo.serverVersion, false, outputDirectory);
          _logger.LogInformation(navisJobOutput, rawRsnFilePath);
          log.Add(navisJobOutput);
        }
        catch (Exception e)
        {
          _logger.LogError(e, e.Message);
          log.Add(e.Message);
        }
      });

      return string.Join("\n", log);
    }
    public string ParralelExportModels(ExportTask task)
    {
      var options = new ParallelOptions() { MaxDegreeOfParallelism = maxThreads };
      // export logs to parallel dump
      var log = new ConcurrentBag<string>();
      Parallel.ForEach(task.Files, options, rawRsnFilePath =>
      {
        try
        {
          var fileInfo = new RsnFileInfo(rawRsnFilePath, task.RsnStructure, task.OutFolder);

          log.Add(rsnCommander.CreateLocalFile(fileInfo));
        }
        catch (Exception e)
        {
          _logger.LogError(e, e.Message);
          log.Add(e.Message);
        }
      });
      return string.Join("\n", log);
    }

    public string BatchExportModels(List<RsnFileInfo> files, bool rsnStructure)
    {
      
      var log = new List<string>();
      try
      {
        var exportModels = files.GroupBy(x => x.serverVersion).ToDictionary(g => g.Key, g => g.ToList());

        foreach (KeyValuePair<int, List<RsnFileInfo>> group in exportModels)
        {
          foreach (var file in group.Value)
          {
            var fileLog = rsnCommander.CreateLocalFile(file);
            log.Add(fileLog);
          }
        }
      }
      catch (Exception e)
      {
        _logger.LogError(e, e.Message);
        log.Add(e.Message);
      }
      return string.Join("\n", log);
    }
  }
}
