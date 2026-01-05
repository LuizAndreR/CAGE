using FluentResults;
using MediatR;
using System.Text.Json.Serialization;

namespace CakeGestao.Application.Features.Auth.Cadastro;

public class CadastroCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Senha { get; set; }
    public required string Role { get; set; }
}
    