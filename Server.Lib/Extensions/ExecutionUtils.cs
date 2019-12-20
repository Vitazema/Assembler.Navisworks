using System;
using System.IO;
using System.Reflection;

namespace Server.Lib.Extensions
{
  public static class ExecutionUtils
  {
    public static string GetAssemblyDirectory()
    {
      var codeBase = Assembly.GetExecutingAssembly().CodeBase;
      // remove File:// at the beginning
      var uri = new UriBuilder(codeBase);
      var path = Uri.UnescapeDataString(uri.Path);
      return Path.GetDirectoryName(path);
    }
  }
}
