public class ConflictError : Exception
{
    public string Errors { get; }

    public ConflictError(string errors)
        : base("Conflito de regra de negócio.")
    {
        Errors = errors;
    }
}
