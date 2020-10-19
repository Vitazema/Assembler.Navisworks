using System;
using System.Diagnostics;
using System.IO;

namespace Server.Assembler.ModelExportService
{
  public class NavisCommander
  {
    public string FindNavisTool(int version)
    {
      string tool = null;
      switch (version)
      {
        case 2017:
          tool = @"C:\Program Files\Autodesk\Navisworks Manage 2017\FiletoolsTaskRunner.exe";
          break;

        case 2019:
          if (File.Exists(@"C:\Program Files\Autodesk\Navisworks Manage 2019\FiletoolsTaskRunner.exe"))
            tool = @"C:\Program Files\Autodesk\Navisworks Manage 2019\FiletoolsTaskRunner.exe";
          else if (File.Exists(@"C:\Program Files\Autodesk\Navisworks Simulate 2019\FiletoolsTaskRunner.exe"))
            tool = @"C:\Program Files\Autodesk\Navisworks Simulate 2019\FiletoolsTaskRunner.exe";
          break;
      }

      return !File.Exists(tool) ? null : tool;
    }

    public string BatchExportToNavis(string configFilePath, int navisVersion, bool isNwd, string exportFolder)
    {
      try
      {
        var log = string.Empty;

        var navisTool = FindNavisTool(navisVersion);
        if (navisTool == null)
          return "Не найден инструмент для выгрузки в Нэвис";

        // Compile cmd config string for NW bat utility
        var navisArgs = $"/i \"{configFilePath}\" /od \"{exportFolder}\" /version {navisVersion}";

        var processInfo = new ProcessStartInfo(navisTool, navisArgs)
        {
          CreateNoWindow = false,
          UseShellExecute = false,
          RedirectStandardError = true,
          RedirectStandardOutput = true
        };

        var process = new Process();
        process.StartInfo = processInfo;

        process.Start();
        process.WaitForExit();
        log = process.StandardOutput.ReadToEnd() + "\n" +
              process.StandardError.ReadToEnd();
        process.Close();
        return log;
      }
      catch (Exception e)
      {
        return e.Message;
      }
    }
  }
}