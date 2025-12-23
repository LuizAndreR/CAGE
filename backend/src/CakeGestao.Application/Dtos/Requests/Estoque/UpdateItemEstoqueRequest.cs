using System.Text.Json.Serialization;

namespace CakeGestao.Application.Dtos.Requests.Estoque;

public class UpdateItemEstoqueRequest
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    [JsonIgnore]
    public int ItemId { get; set; }

    public required string Nome { get; set; }
    public required decimal QuantidadeAtual { get; set; }
    public required string UnidadeMedida { get; set; }
}
