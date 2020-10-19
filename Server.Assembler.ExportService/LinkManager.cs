using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace Server.Assembler.ModelExportService
{
  public class LinkManager
  {
    public static IEnumerable<ExternalFileReference> GetFileLinks(string rsnFilePath)
    {
      var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(rsnFilePath);
      if (!modelPath.ServerPath)
        return null;

      var tryCount = 3;
      while (tryCount > 0)
        try
        {
          var transData = TransmissionData.ReadTransmissionData(modelPath);
          if (transData != null)
          {
            var refs = transData.GetAllExternalFileReferenceIds();
            var extRefs = refs.Select(refId => transData.GetLastSavedReferenceData(refId))
              .Where(extRef =>
                extRef.ExternalFileReferenceType == ExternalFileReferenceType.RevitLink &&
                extRef.GetLinkedFileStatus() == LinkedFileStatus.Loaded &&
                !string.IsNullOrEmpty(extRef.GetAbsolutePath().CentralServerPath));

            return extRefs;
          }
        }
        catch
        {
          --tryCount;
          if (tryCount <= 0)
            throw new Exception("Не удалось прочитать ссылки файла");
        }

      return null;
    }

    //public static void RemoveLibLinksFromRvt(ModelFile modelFileInfo)
    //{
    //  var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(modelFileInfo.RsnFilePath);
    //  var transData = TransmissionData.ReadTransmissionData(modelPath);
    //  foreach (var linkRef in GetFileLinks(modelFileInfo.RsnFilePath))
    //  {
    //    var linkModelPath = linkRef.GetAbsolutePath();
    //    if (linkModelPath.CentralServerPath.Contains(".lib"))
    //    {

    //    }
    //  }
    //  var links = List<>
    //}
  }
}