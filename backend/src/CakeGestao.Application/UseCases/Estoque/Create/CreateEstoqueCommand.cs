using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.UseCases.Estoque.Create;

public class CreateEstoqueCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public required string Nome { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public required string UnidadeMedida { get; set; }
    public decimal Valor { get; set; }
}
