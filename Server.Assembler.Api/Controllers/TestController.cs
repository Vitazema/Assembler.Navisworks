using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Assembler.ModelExportService;

namespace Server.Assembler.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TestController : ControllerBase
  {
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
      _logger = logger;
      rsnCommander = new RsnCommander(_logger);
    }

    public int quote { get; set; } = 10;
    public RsnCommander rsnCommander { get; set; }

    [HttpPost]
    public ActionResult TestAsync([FromBody] string[] requests)
    {
      try
      {
        var maxThreads = 2;
        var rsnFileList = requests.Select(r => new ModelFile(r)).ToList();
        var options = new ParallelOptions {MaxDegreeOfParallelism = maxThreads};
        var logger = new List<string>();

        Parallel.ForEach(rsnFileList, options, file =>
        {
          //var result = JobProcess(file);
          //Console.WriteLine(result);
          //logger.Add(result);
        });
        _logger.LogWarning("something can be happened");
        throw new ArgumentException("wo-po-po-po");
        return Ok(logger);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "something hapaned");
        return BadRequest();
      }
    }

    public string JobProcess(ModelFile file)
    {
      Console.WriteLine($"Job export {file.FullFileName} starts");

      var navisCommander = new NavisCommander();
      var tempConfigFile = Path.GetTempPath() + "temp" + new Random().Next(10000, 99999) + ".txt";

      using (var sw = new StreamWriter(System.IO.File.Create(tempConfigFile)))
      {
        var res = rsnCommander.CreateLocalFile(file, null);
        Console.WriteLine($"File copy: {file.FullFileName} result:{res}");
        sw.WriteLine(file.RvtFilePath);
      }

      var log = navisCommander.BatchExportToNavis(tempConfigFile, 2017, false,
        Path.Combine(Environment.SpecialFolder.UserProfile + "\\Desktop\\выгрузка_тест"));

      Console.WriteLine($"Job {file.FullFileName} ends\nWith result: {log}");

      return log;
    }
  }
}