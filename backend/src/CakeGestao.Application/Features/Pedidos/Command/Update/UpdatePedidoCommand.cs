using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Pedidos.Command.Update;

public class UpdatePedidoCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
    
    [JsonIgnore]
    public int Id { get; set; }

    public required string ClienteNome { get; set; }
    public string? TelefoneCliente { get; set; }
    public string? Descricao { get; set; }
    public DateTime? DataEntrega { get; set; }

    public List<UpdatePedidoItemDto> Itens { get; set; } = new();
}

public class UpdatePedidoItemDto
{
    public int ReceitaId { get; set; }
    public int Quantidade { get; set; }
}