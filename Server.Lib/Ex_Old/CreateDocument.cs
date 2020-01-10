using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Server.Lib.Ex_Old;

namespace Server.Lib.Ex
{
  public static class CreateDocument
  {
    private const string TemplatePath = @"\\picompany.ru\pikp\lib\02_Revit\03_Шаблоны проектов\2017\PIK_BS.rte";
    public static void CreateFileByPath(Application app, string filePath, string templatePath = TemplatePath)
    {
      Document doc = app.NewProjectDocument(templatePath);
      
      if (doc.CanEnableWorksharing() && filePath.IsValidRsnStringPath())
      {
        // TODO: not shure it can auto sort levels and grids
        doc.EnableWorksharing("Общие уровни и стеки", "лолкек");

        CreateWorksets.ByList(doc, CreateWorksets.ParseWorksetList("АР"));

        var opts = new SaveAsOptions();
        var worksharingOpts = new WorksharingSaveAsOptions();
        worksharingOpts.SaveAsCentral = true;
        worksharingOpts.OpenWorksetsDefault = SimpleWorksetConfiguration.AllWorksets;
        opts.SetWorksharingOptions(worksharingOpts);

        var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);

        //TODO: cant relinqush created worksets
        WorksharingUtils.RelinquishOwnership(doc,
          new RelinquishOptions(true) {UserWorksets = true},
          new TransactWithCentralOptions());

        doc.SaveAs(modelPath, opts);
        doc.Close(false);
      }
    }
  }
}
