using System.Linq;
using Autodesk.Revit.DB;

namespace Server.Lib.Extensions
{
  public static class Links
  {
    public static Document FindLinkDoc(this RevitLinkType rvtLinkType, Document parentDoc)
    {
      foreach (Document doc in parentDoc.Application.Documents)
      {
        if (doc.IsValidObject && doc.Title == rvtLinkType.Name.Remove(rvtLinkType.Name.Length - 4))
        {
          return doc;
        }
      }
      return null;
    }

    public static ElementId FindProjectLocation(Document doc)
    {
      var coll = new FilteredElementCollector(doc);
      var projectLocations = coll.OfClass(typeof(ProjectLocation)).ToElements();
      return projectLocations.FirstOrDefault().Id;
    }

    public static bool DropSharedSite(this Document doc)
    {
      var linkInstances = new FilteredElementCollector(doc)
        .OfClass(typeof(RevitLinkInstance))
        .ToList();
      if (linkInstances.Count == 0)
        return false;

      foreach (RevitLinkInstance linkInstance in linkInstances)
      {
        var linkDoc = linkInstance.GetLinkDocument();
        if (linkDoc == null)
          continue;
        var editor = linkDoc.ProjectInformation
          .get_Parameter(BuiltInParameter.EDITED_BY)
          .AsString();

        var site = linkDoc.ActiveProjectLocation;

        //site.SetProjectPosition(null, null);
              
        //var locations = linkDoc.ProjectLocations;
        //var pos = linkDoc.SiteLocation;
              
        //if (editor == app.Username || editor == string.Empty)
        //{
        //  var sharedSiteId = new LinkElementId(linkInstance.Id,
        //    Links.FindProjectLocation(linkInstance.GetLinkDocument()));

        //}

        var pars = linkInstance.Parameters;
      }

      //var col = doc.LookupParameter("Общая площадка").Set("null");
      return true;
    }
  }
}
