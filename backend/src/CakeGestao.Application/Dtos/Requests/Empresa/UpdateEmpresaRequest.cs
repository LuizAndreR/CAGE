using System.Text.Json.Serialization;

namespace CakeGestao.Application.Dtos.Requests.Empresa;

public class UpdateEmpresaRequest
{
    [JsonIgnore]
    public int Id { get; set; }

    public required string Nome { get; set; }
    public required string Endereco { get; set; }
    public required string Status { get; set; }
}
