using CakeGestao.Application.Dtos.Requests.Auth;
using FluentValidation;

namespace CakeGestao.Application.Validators.Auth;

public class LoginUserValidator : AbstractValidator<LoginRequest>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O campo Email é obrigatório.")
            .EmailAddress().WithMessage("O campo Email deve ser um endereço de email válido.");

        RuleFor(x => x.Senha)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O campo Senha é obrigatório.")
            .MinimumLength(6).WithMessage("O campo Senha deve ter no mínimo 6 caracteres.");
    }
}
