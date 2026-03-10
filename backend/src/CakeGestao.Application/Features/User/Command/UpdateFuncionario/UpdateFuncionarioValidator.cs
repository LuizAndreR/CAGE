using CakeGestao.Domain.Enum;
using FluentValidation;

namespace CakeGestao.Application.Features.User.Command.UpdateFuncionario;

public class UpdateFuncionarioValidator : AbstractValidator<UpdateFuncionarioCommand>
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
