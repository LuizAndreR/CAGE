using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.Receitas.Command.Status;

public class UpdateReceitaStatusCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }
    
    [JsonIgnore]
    public int ReceitaId { get; set; }
    
    public bool Status { get; set; }
}