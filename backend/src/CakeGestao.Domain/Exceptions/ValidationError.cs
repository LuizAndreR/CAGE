using FluentResults;

public class ValidationError : Error
{
    public List<string> Errors { get; }

    public ValidationError(List<string> errors)
        : base("Erro de validação.")
    {
        Errors = errors;
    }

    public ValidationError(string error)
        : base("Erro de validação.")
    {
        Errors = new List<string> { error };
    }
}
