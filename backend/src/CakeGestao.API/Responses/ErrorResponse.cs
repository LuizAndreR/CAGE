namespace CakeGestao.API.Responses;

public class ErrorResponse
{
    public string Title { get; set; }
    public int Status { get; set; }
    public string TraceId { get; set; }
    public List<string> Errors { get; set; }

    public ErrorResponse(string title, int status, string traceId, List<string> errors)
    {
        Title = title;
        Status = status;
        TraceId = traceId;
        Errors = errors;
    }
}
