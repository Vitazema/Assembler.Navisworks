using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Assembler.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
      [HttpPost]
      public ActionResult TestAsync([FromBody] string[] requests)
      {
        var log = string.Empty;

        foreach (var request in requests)
        {
          var thread = new Thread(Run.JobProcess);
          thread.Start();
        }

        return Ok();
      }
    }

    public static class Run
    {
      public static void JobProcess()
      {
        Console.WriteLine( " job starting...");
        Thread.Sleep(3000);
        Console.WriteLine(" Job ends");

      }
    }
}