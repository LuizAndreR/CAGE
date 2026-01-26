using CakeGestao.Application.Features.Empresas.Command.Create;
using FluentValidation;

namespace CakeGestao.Application.Features.Empresas.Create;

public class CreateEmpresaValidator : AbstractValidator<CreateEmpresaCommand>
{
    public CreateEmpresaValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
            .MaximumLength(150).WithMessage("O nome da empresa deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Endereco)
            .NotEmpty().WithMessage("O endereço da empresa é obrigatório.")
            .MaximumLength(250).WithMessage("O endereço da empresa deve ter no máximo 250 caracteres.");
    }
}
