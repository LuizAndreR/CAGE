using CakeGestao.Application.Dtos.Requests.Usuario;
using FluentValidation;

namespace CakeGestao.Application.Validators.User;

public class UpdateFuncionarioValidator : AbstractValidator<UpdateFuncionarioUsuarioRequest>
{
    public UpdateFuncionarioValidator()
    {
        RuleFor(u => u.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.");

        RuleFor(u => u.Role)
            .NotEmpty().WithMessage("A função é obrigatória.");
    }
}
