using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Domain.Enum;
using FluentValidation;

namespace CakeGestao.Application.Validators.Empresa;

public class UpdateEmpresaValidator : AbstractValidator<UpdateEmpresaRequest>
{
    public UpdateEmpresaValidator()
    {
        RuleFor(x => x.Nome)
           .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
           .MaximumLength(150).WithMessage("O nome da empresa deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Endereco)
            .NotEmpty().WithMessage("O endereço da empresa é obrigatório.")
            .MaximumLength(250).WithMessage("O endereço da empresa deve ter no máximo 250 caracteres.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("O status é obrigatório.")
            .Must(status => Enum.TryParse<StatusEmpresaEnum>(status, true, out _)).WithMessage($"Status inválido. Valores permitidos: {string.Join(", ", Enum.GetNames(typeof(StatusEmpresaEnum)))}.");
    }
}
