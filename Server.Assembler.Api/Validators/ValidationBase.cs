using System;

namespace Server.Assembler.Api.Validators
{
  public abstract class ValidationBase<T> : IValidation where T : class
  {
    protected ValidationBase(T context)
    {
      if (Context == null)
        throw new ArgumentNullException("context");
      Context = context;
    }

    protected T Context { get; }
    public abstract bool IsValid { get; }
    public abstract string ErrorMessage { get; }

    public void Validate()
    {
      if (!IsValid)
        throw new ValidationException(ErrorMessage);
    }
  }

  public class ValidationException : Exception
  {
    public ValidationException(string errorMessage, params object[] args)
      : base(string.Format(errorMessage, args))
    {
    }
  }
}