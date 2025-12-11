using System.Text.Json.Serialization;

namespace CakeGestao.Application.Dtos.Requests.Usuario;

public class UpdateSenhaUsuarioRequest
{
    [JsonIgnore]
    public int Id { get; set; }

    public required string SenhaAtual { get; set; }
    public required string NovaSenha { get; set; }
}
