using CakeGestao.Application.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Auth.Cadastro;

public class CadastroUserValidator : AbstractValidator<CadastroCommand>
{
   public CadastroUserValidator(IEmpresaRepository empresaRepository)
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

        RuleFor(x => x.Role)
           .NotEmpty().WithMessage("O Role é obrigatório.")
           .Must(role => Enum.TryParse<UserRole>(role, true, out _)).WithMessage($"Role inválido. Valores permitidos: {string.Join(", ", Enum.GetNames(typeof(UserRole)))}.");

        RuleFor(u => u.EmpresaId)
            .NotNull()
               .When(x => x.Role != "Admin")
               .WithMessage("O Id da empresa é obrigatório para usuários não-administradores.")
            .DeveExistirEmpresa(empresaRepository);
    }
}
