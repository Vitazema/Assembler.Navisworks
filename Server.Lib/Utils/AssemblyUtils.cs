using System;
using System.IO;
using System.Reflection;

namespace Server.Lib.Utils
{
  public static class AssemblyUtils
  {
    public static string GetAssemblyDirectory()
    {
      var codebase = Assembly.GetExecutingAssembly().CodeBase;
      var uri = new UriBuilder(codebase);
      var path = Uri.UnescapeDataString(uri.Path);
      return Path.GetDirectoryName(path);
    }
  }
}
