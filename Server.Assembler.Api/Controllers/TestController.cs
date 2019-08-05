﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Assembler.Domain.Entities;
using Server.Assembler.ModelExportService;

namespace Server.Assembler.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TestController : ControllerBase
  {
    [HttpPost]
    public ActionResult TestAsync([FromBody] string[] requests)
    {
      var maxThreads = 2;
      var rsnFileList = requests.Select(r => new RsnFileInfo(r)).ToList();
      var options = new ParallelOptions() {MaxDegreeOfParallelism = maxThreads};
      var logger = new List<string>();

      Parallel.ForEach(rsnFileList, options, file =>
      {
        var result = JobProcess(file);
        Console.WriteLine(result);
        logger.Add(result);
      });

      return Ok(logger);
    }
    public static string JobProcess(RsnFileInfo file)
    {
      Console.WriteLine($"Job export {file.fileFullName} starts");

      var rsnCommander = new RsnCommander();
      var navisCommander = new NavisCommander();
      var tempConfigFile = Path.GetTempPath() + "temp" + new Random().Next(10000, 99999) + ".txt";

      using (var sw = new StreamWriter(System.IO.File.Create(tempConfigFile)))
      {
        var res = rsnCommander.CreateLocalFile(file);
        Console.WriteLine($"File copy: {file.fileFullName} result:{res}");
        sw.WriteLine(file.tempPath);
      }

      var log = navisCommander.BatchExportToNavis(tempConfigFile, 2017, false,
        "C:\\Users\\malozyomovvv\\Desktop\\выгрузка_тест");

      Console.WriteLine($"Job {file.fileFullName} ends\nWith result: {log}");

      return log;
    }
  }
}