public class ValidationError : Exception
{
    public List<string> Errors { get; }

    public ValidationError(List<string> errors)
        : base("Erro de validação.")
    {
        Errors = errors;
    }
}
