using System.Text.Json.Serialization;

namespace CakeGestao.Application.Dtos.Requests.Transacao;

public class CreateTransacaoRequest
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public string Tipo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
