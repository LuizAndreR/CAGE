using CakeGestao.Domain.Enum;
using FluentValidation;

namespace CakeGestao.Application.Features.Empresas.Command.UpdateStatus;

public class UpdateStatusEmpresaValidator : AbstractValidator<UpdateStatusEmpresaCommand>
{
    public UpdateStatusEmpresaValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("O status é obrigatório.")
            .Must(status => Enum.TryParse<StatusEmpresaEnum>(status, true, out _)).WithMessage($"Status inválido. Valores permitidos: {string.Join(", ", Enum.GetNames(typeof(StatusEmpresaEnum)))}.");
    }
}
