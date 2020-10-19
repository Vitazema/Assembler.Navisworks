using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Server.Assembler.ModelExportService;
using Xunit;

namespace Server.Assembler.Tests
{
  public class UnitTest1
  {
    [Fact]
    public void GeneralRsnToolTest()
    {
      var commander = new RsnCommander(null);
      //var file = new RsnFileInfo(@"\\vpp-revittest01.main.picompany.ru\D$\RS17\Prj\0000-Navis\tst\ÑÑÑÐ.rvt");
      //var output = commander.CreateLocalFile(file);
    }

    [Fact]
    public void SimpleJsonRequest()
    {
      using (var sr = new StreamReader("data.json"))
      {
        var json = sr.ReadToEnd();

        var data = JsonConvert.DeserializeObject<List<int>>(json);
      }
    }
  }
}