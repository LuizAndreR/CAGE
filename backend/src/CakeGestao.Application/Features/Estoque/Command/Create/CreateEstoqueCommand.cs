using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Estoque.Command.Create;

public class CreateEstoqueCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public required string Nome { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public decimal QunatidadeMinina { get; set; }
    public required string UnidadeMedida { get; set; }
    public decimal Valor { get; set; }
}
