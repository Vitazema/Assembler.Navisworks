using Server.Assembler.ModelExportService.Services;

namespace Server.Assembler.Tests.Integrational
{
  public class ExportRevitServiceTests
  {
    public ExportService exportService { get; set; }

    //public ExportRevitServiceTests()
    //{
    //  this.exportService = new ExportService();
    //}
    //[Fact]
    //public void CheckOptionalSavingsPathAccepted()
    //{
    //  var fileName = @"\\vpp-revittest01.main.picompany.ru\D$\RS17\Prj\Intergrational tests\SimpleExport.rvt";
    //  var folderPath = @"\\picompany.ru\pikp\Dep\IT\_SR_Public\01_BIM\10_Development\Assembler tests";

    //  var fileInfo = new RsnFileInfo(fileName);
    //  var log = exportService.BatchExportModels(new List<RsnFileInfo>(){fileInfo}, false,folderPath);
    //  var file = Path.Combine(folderPath, "Координация", fileInfo.fullFileName);
    //  Assert.True(File.Exists(file));
    //  File.Delete(file);
    //}
  }
}