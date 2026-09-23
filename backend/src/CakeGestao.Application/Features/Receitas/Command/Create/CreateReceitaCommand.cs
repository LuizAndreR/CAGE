using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Receitas.Command.Create;

public class IngredienteRequestDto
{
    public int ItemId { get; set; } 
    
    public decimal Quantidade { get; set; }
    
    public string UnidadeMedida { get; set; } = string.Empty;
}

public class CreateReceitaCommand : IRequest<Result>
{
    [JsonIgnore] 
    public int EmpresaId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string ModoPreparo { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
    public decimal CustoExtra { get; set; }
    public decimal PercentualMargemLucro { get; set; }

    public List<IngredienteRequestDto> Ingredientes { get; set; } = new();
}