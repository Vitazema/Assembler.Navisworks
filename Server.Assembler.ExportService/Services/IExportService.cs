using System.Collections.Generic;
using Server.Assembler.Domain.Entities;

namespace Server.Assembler.ModelExportService.Services
{
  public interface IExportService
  {
    string BatchExportModelsToFolder(List<RsnFileInfo> files, bool rsnStructure, string outFolder = "");
    string BatchParallelExportModelsToNavis(List<RsnFileInfo> files, bool rsnStructure, string outFolder = "");
  }
}
