using System;
using System.Collections.Generic;

namespace Server.Assembler.Domain.Entities
{
  public class ExportTask
  {
    public ExportTask()
    {
      Id = Guid.NewGuid().ToString();
    }

    public string Id { get; set; }

    /// <summary>
    ///   List of files to be processed for Navis export. Accept any file format
    ///   that can be directed to physical .rvt file
    /// </summary>
    public List<string> Files { get; set; }

    /// <summary>
    ///   Files destination
    /// </summary>
    public string OutFolder { get; set; }

    /// <summary>
    ///   Define if RSN folder structure will be preserve for exported files
    /// </summary>
    public bool RsnStructure { get; set; }

    /// <summary>
    ///   Operate given prority in loop quote. Usually division number.
    /// </summary>
    public int QuoteMarker { get; set; }
  }
}