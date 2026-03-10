using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Financeiro.Query.GetAll;

public class GetAllTransacaoValidator : AbstractValidator<GetAllTransacaoQuery>
{
    public GetAllTransacaoValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);
    }
}