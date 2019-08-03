using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Server.Lib.Extensions
{
  public static class ExecutionUtils
  {
    public static string GetAssemblyDirectory()
    {
      string codeBase = Assembly.GetExecutingAssembly().CodeBase;
      // remove File:// at the beginning
      var uri = new UriBuilder(codeBase);
      string path = Uri.UnescapeDataString(uri.Path);
      return Path.GetDirectoryName(path);
    }
  }
}
