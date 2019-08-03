using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Lib.Extensions
{
  public static class StringUtils
  {
    public static bool ContainsAny(this string pile, params string[] needles)
    {
      return needles.Any(pile.Contains);
    }
  }
}
