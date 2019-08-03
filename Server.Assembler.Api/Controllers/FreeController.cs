using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Assembler.FreeService;

namespace Server.Assembler.Api.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class FreeController : ControllerBase
  {
    private readonly IFreeService freeService;

    public FreeController(IFreeService freeService)
    {
      this.freeService = freeService;
    }

    //[HttpPost]
    //public ActionResult<IEnumerable<string>> RelinquishModelByPath(
    //  [FromForm] string user,
    //  [FromForm] string rawFilePath)
    //{
      
    //}
  }
}