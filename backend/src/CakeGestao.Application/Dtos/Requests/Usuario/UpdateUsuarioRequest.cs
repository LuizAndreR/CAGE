using System.Text.Json.Serialization;

namespace CakeGestao.Application.Dtos.Requests.Usuario;

public class UpdateUsuarioRequest
{
    [JsonIgnore]
    public int Id { get; set; }

    public required string Nome { get; set; }
    public required string Email { get; set; }
}