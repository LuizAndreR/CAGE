using FluentValidation;

namespace CakeGestao.Application.Features.User.Command.UpdateSenhaUsuario;

public class UpdateSenhaUsuarioValidator : AbstractValidator<UpdateSenhaUsuarioCommand>
{
    public UpdateSenhaUsuarioValidator()
    {
        RuleFor(x => x.SenhaAtual)
            .NotEmpty().WithMessage("A senha atual não pode estar vazia.")
            .MinimumLength(6).WithMessage("A senha atual deve ter pelo menos 6 caracteres.");

        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("A nova senha não pode estar vazia.")
            .MinimumLength(6).WithMessage("A nova senha deve ter pelo menos 6 caracteres.");
    }
}
