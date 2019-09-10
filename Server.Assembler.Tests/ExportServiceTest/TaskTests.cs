using Newtonsoft.Json;
using Server.Assembler.Domain.Entities;
using Xunit;

namespace Server.Assembler.Tests.ExportServiceTest
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
		          ""vpp-revittest01.main.picompany.ru\\0000-Navis\\kek.rvt"",
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
