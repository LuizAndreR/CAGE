using CakeGestao.Application.Dtos.Requests.Empresa;
using FluentValidation;

namespace CakeGestao.Application.Validators.Empresa;

public class CreateEmpresaValidator : AbstractValidator<CreateEmpresaRequest>
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
