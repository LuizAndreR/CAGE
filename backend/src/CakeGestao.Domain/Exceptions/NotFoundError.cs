public class NotFoundError : Exception
{
    public string Errors { get; }

    public NotFoundError(string errors)
        : base("Request não encontrado.")
    {
        Errors = errors;
    }
}
