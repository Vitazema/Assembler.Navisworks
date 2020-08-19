using System.Collections.Generic;
using Server.Assembler.Domain.Entities;

namespace Server.Assembler.ModelExportService.Services
{
  public interface IExportService
  {
    string BatchExportModels(List<RsnFileInfo> files, bool rsnStructure);
    string ParallelExportModelsToNavis(ExportTask task);
    string ParralelExportModels(ExportTask task);
  }
}
