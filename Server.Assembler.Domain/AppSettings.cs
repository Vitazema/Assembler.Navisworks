using Microsoft.Extensions.Logging;

namespace Server.Assembler.Domain
{
  public class Appsettings
  {
    public Logging Logging { get; set; }
    public Perfomance Perfomance { get; set; }
  }

  public class Logging
  {
    public string LogstashServer { get; set; }
  }

  public class Perfomance
  {
    public int MaxDegreeOfParallelism { get; set; }
    public bool AutoThreads { get; set; }
  }
}
