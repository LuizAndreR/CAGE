using CakeGestao.Application.Dtos.Requests.Auth;
using FluentValidation;

namespace CakeGestao.Application.Validators.Auth;

public class CadastroUserValidator : AbstractValidator<CadastroRequest>
{
   public CadastroUserValidator()
   {
        RuleFor(u => u.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.");

        RuleFor(u => u.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O Email é obrigatório.")
            .EmailAddress().WithMessage("O Email fornecido não é válido.");

        RuleFor(u => u.Senha)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");
    }
}
