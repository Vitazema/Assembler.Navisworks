namespace Server.Assembler.Api.Validators
{
  public interface IValidation
  {
    bool IsValid { get; }
    string ErrorMessage { get; }
    void Validate();
  }
}