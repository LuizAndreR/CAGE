using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Receitas.Command.Update;

public class UpdateReceitaCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
    
    [JsonIgnore]
    public int Id { get; set; }    
    
    public required string Nome { get; set; }
    public required string ModoPreparo { get; set; }
    public decimal PrecoVenda { get; set; }

    public List<UpdateIngredienteDto> Ingredientes { get; set; } = new();
}

public class UpdateIngredienteDto
{
    public int ItemId { get; set; }
    public decimal Quantidade { get; set; }
    public required string UnidadeMedida { get; set; }
}
