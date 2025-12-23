using System.Text.Json.Serialization;

namespace CakeGestao.Application.Dtos.Requests.Estoque;

public class RemoveQuantidadeEstoqueRequest
{
    [JsonIgnore]
    public required int EmpresaId { get; set; }
    [JsonIgnore]
    public required int ItemId { get; set; }
    public required decimal QuantidadeARemover { get; set; }
}
