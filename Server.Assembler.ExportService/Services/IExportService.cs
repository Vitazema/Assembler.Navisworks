using System.Collections.Generic;
using Server.Assembler.Domain.Entities;

namespace Server.Assembler.ModelExportService.Services
{
  public interface IExportService
  {
    string ExportModelToNavis(RsnFileInfo file);
    string BatchExportModelsToNavis(List<RsnFileInfo> files, string outFolder = "");
    string BatchExportModelToNavisParallel(List<RsnFileInfo> files, bool rsnStructure, string outFolder = "");

    string BatchModelExport(List<RsnFileInfo> files);
    string RevitModelExport(RsnFileInfo file, string folder = "");
  }
}
