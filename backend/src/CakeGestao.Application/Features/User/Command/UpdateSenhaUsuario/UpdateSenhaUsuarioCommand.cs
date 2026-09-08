using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace CakeGestao.Application.Features.User.Command.UpdateSenhaUsuario;

public class UpdateSenhaUsuarioCommand : IRequest<Result>
{
    [JsonIgnore]
    public int EmpresaId { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    public required string SenhaAtual { get; set; }
    public required string NovaSenha { get; set; }
}
