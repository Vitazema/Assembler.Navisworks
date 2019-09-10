using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Assembler.Domain.Entities;
using Server.Assembler.ModelExportService.Services;
using Server.Lib.RevitServer;

namespace Server.Assembler.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class NavisController : ControllerBase
  {
    private readonly ILogger<NavisController> _logger;
    private readonly IExportService navisService;

    public NavisController(ILogger<NavisController> logger, IExportService navisService)
    {
      _logger = logger;
      this.navisService = navisService;
    }

    [HttpPost]
    [Route("parallel")]
    public ActionResult<string> BatchPalallelExportToNavisworks(ExportTask task)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          return BadRequest(ModelState);
        }

        var filePaths = task.Files;

        var result = string.Empty;
        var rsnFiles = new List<RsnFileInfo>();

        if (filePaths.Count == 0)
          return BadRequest();

        foreach (var filePath in filePaths)
        {
          var file = new RsnFileInfo(filePath);

          if (file.rsnFilePath == null)
            result += $"\nПуть для файла не может быть обработан: {file}";

          if (!file.rsnFilePath.IsValidForRsnModelPath())
            result += $"\n Не валидный файл для RSN: {file}";

          rsnFiles.Add(file);
        }

        return Ok(result + "\n"
                         + navisService.BatchParallelExportModelsToNavis(
                           rsnFiles,
                           task.RsnStructure,
                           task.OutFolder));
      }
      catch (Exception e)
      {
        _logger.LogError(e, "Task execution error {task}", task);
        return BadRequest("!!! Ошибка !!!\n" + e.Message);
      }
    }


    //[HttpPost]
    //[Route("batch")]
    //public ActionResult<string> BatchExportToNavisworks(ExportTask task)
    //{
    //  try
    //  {
    //    if (!ModelState.IsValid)
    //    {
    //      return BadRequest(ModelState);
    //    }

    //    var filePaths = task.Files;

    //    var result = string.Empty;
    //    var rsnFiles = new List<RsnFileInfo>();

    //    if (filePaths.Count == 0)
    //      return BadRequest();

    //    foreach (var filePath in filePaths)
    //    {
    //      var file = new RsnFileInfo(filePath);

    //      if (file.rsnFilePath == null)
    //        result += $"\nПуть для файла не может быть обработан: {file}";

    //      if (!file.rsnFilePath.IsValidForRsnModelPath())
    //        result += $"\n Не валидный файл для RSN: {file}";

    //      rsnFiles.Add(file);
    //    }

    //    return Ok(result + "\n"
    //      + navisService.BatchExportModelsToFolder(rsnFiles, task.OutFolder));
    //  }
    //  catch (Exception e)
    //  {
    //    return BadRequest("Ошибка:\n" + e.Message);
    //  }
    //}

  }
}