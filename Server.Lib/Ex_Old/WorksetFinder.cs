using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;

namespace Server.Lib.Ex_Old
{
  public static class WorksetFinder
  {
    public static IEnumerable<string> FindBase(Document document)
    {
      var baseElements = new FilteredElementCollector(document)
        .WherePasses(new LogicalOrFilter(
          new List<ElementFilter>()
          {
            new ElementCategoryFilter(BuiltInCategory.OST_Grids),
            new ElementCategoryFilter(BuiltInCategory.OST_Levels)
          }))
        .WhereElementIsNotElementType()
        .ToElements();
      var worksetsOfBaseElements =
        baseElements.Select(e => e.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)?.AsValueString());

      // Unique worksets
      return new HashSet<string>(worksetsOfBaseElements);
    }
    public static IEnumerable<string> FindNestedLinks(Document document)
    {
      var col = new FilteredElementCollector(document)
        .OfClass(typeof(Instance))
        .OfCategory(BuiltInCategory.OST_RvtLinks);

      var nestedLinks = new List<RevitLinkInstance>();

      foreach (var element in col) {
        if (element is RevitLinkInstance ins) {

          RevitLinkType linkType = document.GetElement(ins.GetTypeId()) as RevitLinkType;

          if (linkType?.AttachmentType == AttachmentType.Attachment) {
            nestedLinks.Add(ins);
          }
        }
      }

      // FindNestedLinks all user worksets 

      //      FilteredWorksetCollector worksets
      //        = new FilteredWorksetCollector( doc )
      //          .OfKind( WorksetKind.UserWorkset );
      var nestedLinksWorksetNames = new List<string>();

      // Get workset datatable
      var wsTable = document.GetWorksetTable();
//      var lstPrewiew = WorksharingUtils.GetUserWorksetInfo(document.GetWorksharingCentralModelPath());

      foreach (var nestedLink in nestedLinks)
      {
          var nestedLinkWorkset = wsTable.GetWorkset(nestedLink.WorksetId);
          nestedLinksWorksetNames.Add(nestedLinkWorkset.Name);
          Debug.Write((string) nestedLinkWorkset.Name);
      }

      return nestedLinksWorksetNames;
    }
  }
}
