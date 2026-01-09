using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Financeiro.Query.Get;

public class GetTransacaoValidator : AbstractValidator<GetTransacaoQuery>
{
    public GetTransacaoValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID da transação inválido.");
    }
}