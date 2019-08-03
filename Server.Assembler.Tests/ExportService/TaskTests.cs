using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Assembler.Domain.Entities;
using Xunit;

namespace Server.Assembler.Tests
{
  public class TaskTests
  {
    [Fact]
    public void RestTaskComplied()
    {
      var jsonData = @"
          {
	          ""OutFolder"":""lel\\kekek"",
	          ""Files"":
	          [
		          ""\\\\vpp-revittest01.main.picompany.ru\\0000-Navis\\lol.rvt"",
		          ""\\\\vpp-revittest01.main.picompany.ru\\0000-Navis\\kek.rvt"",
		          ""\\\\vpp-revittest01.main.picompany.ru\\0000-Navis\\sdfasdfsadfek.rvt"",
		          ""\\\\vpp-revit19_1.main.picompany.ru\\9999_Отдел внедрения BIM\\Relinquish\\sublink.rvt""
	          ]
	          
          }
        ";

      var data = JsonConvert.DeserializeObject<ExportTask>(jsonData);
      Assert.NotNull(data);
    }
  }
}
