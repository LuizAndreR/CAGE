using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Receitas.Command.Delete;

public class DeleteReceitaCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
    
    [JsonIgnore]
    public int ReceitaId { get; set; }
}