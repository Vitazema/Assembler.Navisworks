using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Assembler.Domain.Entities
{
    public class ExportTask
    {
      public string OutFolder { get; set; }
      public bool RsnStructure { get; set; }
      public List<string> Files { get; set; }
    }
}
