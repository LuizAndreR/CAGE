using CakeGestao.Domain.Enum;
using FluentValidation;

namespace CakeGestao.Application.Features.Empresas.Command.Update;

public class UpdateEmpresaValidator : AbstractValidator<UpdateEmpresaCommand>
{
    public UpdateEmpresaValidator()
    {
        RuleFor(x => x.Nome)
           .Cascade(CascadeMode.Stop)
           .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
           .MaximumLength(150).WithMessage("O nome da empresa deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Endereco)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O endereço da empresa é obrigatório.")
            .MaximumLength(250).WithMessage("O endereço da empresa deve ter no máximo 250 caracteres.");
    }
}
