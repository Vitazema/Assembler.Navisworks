using System.IO;
using Newtonsoft.Json;
using Xunit;

namespace Server.Assembler.Tests
{
  public class RsnFileConversionTests
  {
    [Fact]
    public void ConvertCommonFileToRsnFileInfo()
    {
      var fileName = @"\\vpp-revittest01.main.picompany.ru\D$\RS17\Prj\Intergrational tests\SimpleExport.rvt";

      //var file = new RsnFileInfo(fileName);
      //var success = file.rsnFilePath;
      //Assert.NotNull(success);
    }

    [Fact]
    public void ConvertCommonFileToRsnFileInfoCase1()
    {
      var fileName = @"\\\\vpp-revittest01.main.picompany.ru\\0000-Navis\\lol.rvt";

      //var file = new RsnFileInfo(fileName);
      //var success = file.rsnFilePath;
      //Assert.NotNull(success);
    }

    [Fact]
    public void ParseServerConfigFile()
    {
      using (var sr = new StreamReader("RsnServers.json"))
      {
        var json = sr.ReadToEnd();
        var obj = JsonConvert.DeserializeObject<RsnServers>(json).RsnServerList;
        Assert.True(obj[2017].Contains("vpp-revittest01"));
      }
    }
  }
}