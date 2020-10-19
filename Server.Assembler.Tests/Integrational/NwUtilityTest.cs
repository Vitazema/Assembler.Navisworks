using System;
using System.Diagnostics;
using System.IO;
using Server.Assembler.ModelExportService.Services;
using Xunit;

namespace Server.Assembler.Tests.Integrational
{
  public class NwUtilityTest
  {
    public NwUtilityTest()
    {
      exportService = new ExportService();
    }

    public ExportService exportService { get; set; }

    [Fact]
    public void CompileValidArgumentCommandForNwUtility()
    {
      if (!File.Exists(@"C:\Program Files\Autodesk\Navisworks Manage 2019\FiletoolsTaskRunner.exe"))
        throw new Exception("NW не установлен");

      var navisTool = @"C:\Program Files\Autodesk\Navisworks Manage 2019\FiletoolsTaskRunner.exe";

      var configFilePath = @"C:\Coding\Assembler\Server.Assembler.Tests\bin\Debug\netcoreapp2.2\Integrational\temp.txt";

      var exportFolder = "\\\\picompany.ru\\pikp\\Dep\\IT\\_SR_Public\\01_BIM\\20_Выгрузки\\_Tests";

      // Compile cmd config string for NW bat utility
      var navisArgs = $"/i \"{configFilePath}\" /od \"{exportFolder}\" /version {2019}";

      var processInfo = new ProcessStartInfo(navisTool, navisArgs)
      {
        CreateNoWindow = false,
        UseShellExecute = false,
        RedirectStandardError = true,
        RedirectStandardOutput = true
      };

      var process = new Process();
      process.StartInfo = processInfo;

      if (process.Start())
      {
        process.WaitForExit();
        var log = process.StandardOutput.ReadToEnd();

        process.Close();
      }
    }
  }
}