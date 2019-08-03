using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Assembler.Api.Validators
{
  public interface IValidation
  {
    bool IsValid {get;}
    void Validate();
    string ErrorMessage {get;}
  }
}
