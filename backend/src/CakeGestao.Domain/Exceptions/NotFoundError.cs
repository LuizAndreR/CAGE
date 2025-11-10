using FluentResults;

public class NotFoundError : Error
{
    public string Errors { get; }

    public NotFoundError(string errors)
        : base("Request não encontrado.")
    {
        Errors = errors;
    }
}
