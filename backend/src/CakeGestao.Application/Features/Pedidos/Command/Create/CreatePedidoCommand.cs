using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Pedidos.Command.Create;

public class CreatePedidoCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
    
    public required string ClienteNome { get; set; }
    public string? TelefoneCliente { get; set; }
    public string? Descricao { get; set; }
    public DateTime? DataEntrega { get; set; }

    public List<ItemPedidoDto> Itens { get; set; } = new();
}

public class ItemPedidoDto
{
    public int ReceitaId { get; set; }
    public int Quantidade { get; set; }
}