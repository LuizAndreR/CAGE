using System.Text.Json.Serialization;

namespace CakeGestao.Application.Dtos.Requests.Estoque;

public class ItemEstoqueRequest
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public int ItemId { get; set; }
}
