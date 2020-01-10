using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.DB;

namespace Server.Lib.Ex_Old
{
  public static class CreateWorksets
  {
    public const string worksetConfigFile =
      @"\\picompany.ru\pikp\lib\_CadSettings\02_Revit\13. Settings\10_WorksetTool\worksets.txt";
    public static List<Workset> ByList(Document document, IEnumerable<string> worksets)
    {
      var newWorksets = new List<Workset>();
      // Worksets can only be created in a document with worksharing enabled
      if (document.IsWorkshared)
      {
        using (Transaction trans = new Transaction(document, "Созданы рабочие наборы"))
        {
          trans.Start();
          foreach (var workset in worksets)
          {
            // Workset name must not be in use by another workset
            if (WorksetTable.IsWorksetNameUnique(document, workset))
            {
              newWorksets.Add(Workset.Create(document, workset));
            }
          }

          trans.Commit();
        }
      }
      return newWorksets;
    }


    public static Dictionary<string, List<string>> ParseWorksetList()
    {
      var worksets = new Dictionary<string, List<string>>();
      var category = "";

      foreach (var line in File.ReadAllLines(worksetConfigFile))
      {
        if (line.StartsWith("="))
        {
          category = line.Remove(0, 1);
        }
        else
        {
          if (!worksets.ContainsKey(category))
          {
            worksets.Add(category, new List<string>());
          }
          if (line.Trim().Length > 0)
          {
            worksets[category].Add(line.Trim());
          }
        }
      }
      return worksets;
    }
    public static List<string> ParseWorksetList(string section)
    {

      return ParseWorksetList()[section];
    }
  }
}
