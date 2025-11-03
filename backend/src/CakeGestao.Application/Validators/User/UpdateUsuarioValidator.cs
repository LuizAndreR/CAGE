using CakeGestao.Application.Dtos.Requests.Usuario;
using FluentValidation;

namespace CakeGestao.Application.Validators.User;

public class UpdateUsuarioValidator : AbstractValidator<UpdateUsuarioRequest>
{
    public UpdateUsuarioValidator()
    {
        RuleFor(u => u.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.");

        RuleFor(u => u.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O Email é obrigatório.")
            .EmailAddress().WithMessage("O Email fornecido não é válido.");
    }
}