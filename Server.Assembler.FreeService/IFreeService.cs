namespace Server.Assembler.FreeService
{
  public interface IFreeService
  {
    string FreeProject(string user, string filePath);
    void StartTask(string rvtExe, string user, string filePath);
  }
}