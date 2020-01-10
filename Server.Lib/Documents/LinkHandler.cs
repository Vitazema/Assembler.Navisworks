using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;

namespace Server.Lib.Documents
{
  public class LinkHandler
  {
    public List<RevitLinkType> linkTypes = new List<RevitLinkType> { };

    public LinkHandler(Document doc)
    {
      var collector = new FilteredElementCollector(doc);
      foreach (var element in collector.OfClass(typeof(RevitLinkType)))
      {
        var linkType = (RevitLinkType) element;
        var extFileRef = linkType.GetExternalFileReference();
        if (extFileRef != null)
          linkTypes.Add(linkType);
      }
    }

    public LinkHandler(Document doc, ICollection<RevitLinkInstance> linkInstances)
    {
      foreach (var link in linkInstances)
      {
        var collector = new FilteredElementCollector(doc);
        foreach (var element in collector.OfClass(typeof(RevitLinkType)))
        {
          var linkType = (RevitLinkType) element;
          if (link.Name.Contains(linkType.Name))
          {
            var extFileRef = linkType.GetExternalFileReference();
            if (extFileRef != null)
              linkTypes.Add(linkType);
          }
        }
      }
    }

    public void UnloadLinks()
    {
      foreach (var revitLinkType in linkTypes)
      {
        try
        {
          if (revitLinkType.GetExternalFileReference().GetLinkedFileStatus() == LinkedFileStatus.Loaded)
            // TODO: save changes on shared positining
            revitLinkType.Unload(null);
        }
        catch (Exception e)
        {
          Debug.Write(e);
        }

      }
    }

    public void ReloadUsedLinks()
    {
      foreach (var revLinkType in linkTypes)
        if (revLinkType.GetExternalFileReference().GetLinkedFileStatus() == LinkedFileStatus.Unloaded)
          // revLinkType.Load();
          revLinkType.Reload();
    }
  }
}
