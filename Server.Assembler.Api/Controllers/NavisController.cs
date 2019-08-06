using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IExportService navisService;

    public NavisController(IExportService navisService)
    {
      this.navisService = navisService;
    }
    // Get export job api/export
    [HttpPost]
    public ActionResult<string> ExportToNavisworks([FromForm] string filePath)
    {
      try
      {
        if (!ModelState.IsValid)
        {
          return BadRequest(ModelState);
        }

        var file = new RsnFileInfo(filePath);

        if (file.rsnFilePath == null)
        {
          return new BadRequestObjectResult("Путь не может быть прочитан");
        }

        if (!file.rsnFilePath.IsValidForRsnModelPath())
        {
          return BadRequest(ModelState);
        }

        var result = navisService.ExportModelToNavis(file);
        return Ok(result);
      }
      catch (Exception e)
      {
        return BadRequest("Ошибка:\n" + e.Message);
      }
    }

    [HttpPost]
    [Route("batch")]
    public ActionResult<string> BatchExportToNavisworks(ExportTask task)
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
          + navisService.BatchExportModelsToNavis(rsnFiles, task.OutFolder));
      }
      catch (Exception e)
      {
        return BadRequest("Ошибка:\n" + e.Message);
      }
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
                         + navisService.BatchExportModelToNavisParallel(
                           rsnFiles,
                           task.RsnStructure,
                           task.OutFolder));

      }
      catch (Exception e)
      {
        return BadRequest("!!! Ошибка !!!\n" + e.Message);
      }
    }
  }
}