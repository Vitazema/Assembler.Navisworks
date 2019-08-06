using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Assembler.Domain.Entities;
using Server.Assembler.ModelExportService.Services;
using Server.Lib.RevitServer;

namespace Server.Assembler.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RsnController : ControllerBase
  {
    private readonly IExportService exportService;

    public RsnController(IExportService exportService)
    {
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
        var file = new RsnFileInfo(filePath);
        rsnFiles.Add(file);

        if (file.rsnFilePath == null)
          result += $"\nПуть для файла не может быть обработан: {file}";

        if (!file.rsnFilePath.IsValidForRsnModelPath())
          result += $"\n Не валидный файл для RSN: {file}";
      }

      return Ok(result + "\n" + exportService.BatchExportModelsToFolder(rsnFiles, task.RsnStructure));
    }
  }
}