using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Financeiro.Command.Cancelamento;

public class CancelTransacaoValidator : AbstractValidator<CancelTransacaoCommand>
{
    public CancelTransacaoValidator(IEmpresaRepository repository)
    {
        RuleFor(x => x.TransacaoId)
            .GreaterThan(0).WithMessage("ID da transação inválido.");

        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(repository);

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("O motivo do cancelamento é obrigatório.")
            .MaximumLength(500).WithMessage("O motivo do cancelamento deve ter no máximo 500 caracteres.");
    }
}
