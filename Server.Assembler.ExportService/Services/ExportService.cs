using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Assembler.Domain;
using Server.Assembler.Domain.Entities;

namespace Server.Assembler.ModelExportService.Services
{
  public class ExportService : IExportService
  {
    public const string defaultNavisExportFolder = @"\\picompany.ru\pikp\NAVIS-EXP\_Экспорт";
    private readonly ILogger _logger;
    private readonly NavisCommander navisCommander;
    private readonly RsnCommander rsnCommander;
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

    public int maxThreads { get; set; } = Environment.ProcessorCount;

    //// TODO: check if can catch configuration in runtime
    //// when service is called
    //public ExportService(IOptions<Performance> config) : this()
    //{
    //}

    public string ParallelExportModelsToNavis(ExportTask task)
    {
      var options = new ParallelOptions {MaxDegreeOfParallelism = maxThreads};
      var log = new ConcurrentBag<string>();
      Parallel.ForEach(task.Files, options, sysFilePath =>
      {
        try
        {
          var file = new ModelFile(sysFilePath);
          file.TryValidate();

          //TODO: remove temp files and call Roamer with arguments instead
          var tempConfigFile = Path.GetTempPath() + "temp" + new Random().Next(1000000, 9999999) + ".txt";

          // If file not a local .rvt version
          if (file.RvtFilePath.Length == 0)
          {
            string rvtFileDestination;

            if (task.OutFolder != null)
              rvtFileDestination = Path.Combine(task.OutFolder,
                task.RsnStructure ? file.ProjectFileFullPathWithoutServername : file.FullFileName);
            else
              rvtFileDestination = Path.Combine(Path.GetTempPath(),
                file.ServerName,
                task.RsnStructure ? file.ProjectFileFullPathWithoutServername : file.FullFileName);

            file.RvtFilePath = rsnCommander.CreateLocalFile(file, rvtFileDestination);
          }

          using (var sw = new StreamWriter(File.Create(tempConfigFile), Encoding.UTF8))
          {
            sw.WriteLine(file.RvtFilePath);
          }

          var outputDirectory = task.OutFolder;
          if (task.RsnStructure)
            outputDirectory = Path.Combine(task.OutFolder, file.ProjectDirectory);

          var navisJobOutput =
            navisCommander.BatchExportToNavis(tempConfigFile, file.RevitVersion, false, outputDirectory);
          _logger.LogInformation(navisJobOutput, sysFilePath);
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
      var options = new ParallelOptions {MaxDegreeOfParallelism = maxThreads};
      // export logs to parallel dump
      var log = new ConcurrentBag<string>();
      Parallel.ForEach(task.Files, options, sysRsnFilePath =>
      {
        try
        {
          var file = new ModelFile(sysRsnFilePath);
          file.TryValidate();

          if (file.RvtFilePath == null)
          {
            var destinationRvtFullPath = task.RsnStructure
              ? Path.Combine(task.OutFolder, file.ProjectFileFullPathWithoutServername)
              : Path.Combine(task.OutFolder, file.FullFileName);

            log.Add(rsnCommander.CreateLocalFile(file, destinationRvtFullPath));
          }
          else
          {
            log.Add($".rvt file {sysRsnFilePath} already exist");
          }
        }
        catch (Exception e)
        {
          _logger.LogError(e, e.Message);
          log.Add(e.Message);
        }
      });
      return string.Join("\n", log);
    }

    //public string BatchExportModels(List<ModelFile> files, bool rsnStructure)
    //{

    //  var log = new List<string>();
    //  try
    //  {
    //    var exportModels = files.GroupBy(x => x.RevitVersion).ToDictionary(g => g.Key, g => g.ToList());

    //    foreach (KeyValuePair<int, List<ModelFile>> group in exportModels)
    //    {
    //      foreach (var file in group.Value)
    //      {
    //        var fileLog = rsnCommander.CreateLocalFile(file);
    //        log.Add(fileLog);
    //      }
    //    }
    //  }
    //  catch (Exception e)
    //  {
    //    _logger.LogError(e, e.Message);
    //    log.Add(e.Message);
    //  }
    //  return string.Join("\n", log);
    //}
  }
}