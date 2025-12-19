using System.Text.Json.Serialization;

namespace CakeGestao.Application.Dtos.Requests.Estoque;

public class CreateEstoqueRequest
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public required string Nome { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public required string UnidadeMedida { get; set; }
    public required decimal Valor { get; set; }
}
