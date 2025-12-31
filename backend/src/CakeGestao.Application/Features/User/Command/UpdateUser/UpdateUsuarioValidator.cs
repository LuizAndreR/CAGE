using FluentValidation;

namespace CakeGestao.Application.Features.User.Command.UpdateUser;

public class UpdateUsuarioValidator : AbstractValidator<UpdateUsuarioCommand>
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