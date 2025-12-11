using CakeGestao.Application.Dtos.Requests.Usuario;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Enun;
using FluentValidation;

namespace CakeGestao.Application.Validators.User;

public class UpdateFuncionarioValidator : AbstractValidator<UpdateFuncionarioUsuarioRequest>
{
    public UpdateFuncionarioValidator()
    {
        RuleFor(u => u.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.");

        RuleFor(x => x.Role)
            .IsEnumName(typeof(UserRole), caseSensitive: false)
            .WithMessage($"Role inválido. Valores aceitos: {string.Join(", ", Enum.GetNames(typeof(UserRole)))}");
    }
}
