using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Receitas.Command.Update;

public class UpdateReceitaCommand : IRequest<Result>
{
    [JsonIgnore]
    public int Empresa { get; set; }
    
    [JsonIgnore]
    public int IdReceita { get; set; }    
    
    public required string Nome { get; set; }
    public required string ModoPreparo { get; set; }
    public decimal PrecoVenda { get; set; }
} 
