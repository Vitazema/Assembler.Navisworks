using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Assembler.Api.Validators
{
  public abstract class ValidationBase<T> : IValidation where T : class
  {
    public abstract bool IsValid {get;}
    public abstract string ErrorMessage {get;}
    protected T Context {get; private set;}
    protected ValidationBase(T context)
    {
      if (Context == null)
        throw new ArgumentNullException("context");
      Context = context;
    }
    public void Validate()
    {
      if (!IsValid)
        throw new ValidationException(ErrorMessage);
    }
  }

  public class ValidationException : Exception
  {
    public ValidationException(string errorMessage, params object[] args)
      : base(string.Format(errorMessage, args)) {}
  }
}
