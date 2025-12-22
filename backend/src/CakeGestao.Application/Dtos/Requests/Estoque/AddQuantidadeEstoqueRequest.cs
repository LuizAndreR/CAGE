using System.Text.Json.Serialization;

namespace CakeGestao.Application.Dtos.Requests.Estoque;

public class AddQuantidadeEstoqueRequest
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    [JsonIgnore]
    public int EstoqueId { get; set; }

    public decimal QuantidadeAdicionar { get; set; }
    public decimal Valor { get; set; }
}
