using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Domain.Enum;
using FluentValidation;

namespace CakeGestao.Application.Validators.Empresa;

public class UpdateStatusEmpresaValidator : AbstractValidator<UpdateStatusEmpresaRequest>
{
    public UpdateStatusEmpresaValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("O status é obrigatório.")
            .Must(status => Enum.TryParse<StatusEmpresaEnum>(status, true, out _)).WithMessage($"Status inválido. Valores permitidos: {string.Join(", ", Enum.GetNames(typeof(StatusEmpresaEnum)))}.");
    }
}
