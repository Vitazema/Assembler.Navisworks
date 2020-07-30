using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Assembler.Domain.Entities;
using Server.Assembler.ModelExportService.Services;
using Server.Lib.RevitServer;

namespace Server.Assembler.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RsnController : ControllerBase
  {
    private readonly ILogger<RsnController> _logger;
    private readonly IExportService exportService;

    public RsnController(ILogger<RsnController> logger, IExportService exportService)
    {
      _logger = logger;
      this.exportService = exportService;
    }

    [HttpPost]
    [Route("batch")]
    public ActionResult<string> BatchExportModel(ExportTask task)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      if (task.Files.Count == 0)
        return BadRequest();

      var result = string.Empty;
      var rsnFiles = new List<RsnFileInfo>();

      foreach (var filePath in task.Files)
      {
        var file = new RsnFileInfo(filePath, task.RsnStructure, task.OutFolder);
        rsnFiles.Add(file);

        if (file.rsnFilePath == null)
          result += $"\nПуть для файла не может быть обработан: {file}";

        if (!file.rsnFilePath.IsValidForRsnModelPath())
          result += $"\n Не валидный файл для RSN: {file}";
      }

      return Ok(result + "\n" + exportService.BatchExportModels(rsnFiles, task.RsnStructure));
    }

    [HttpPost]
    [Route("parallel")]
    public ActionResult<string> ParallelExportModel(ExportTask task)
    {
      try
      {
        if (!ModelState.IsValid)
          return BadRequest(ModelState);

        if (task.Files.Count == 0)
          return BadRequest("Не указаны файлы для выгрузки");

        var result = string.Empty;

        var exportTaskLog = exportService.ParralelExportModels(task);

        return Ok(result + "\n" + exportTaskLog);
      }
      catch (Exception e)
      {
        _logger.LogCritical(e, "Task execution error {task}", task);
        return BadRequest("Error: " + e.Message);
      }
    }
  }
}