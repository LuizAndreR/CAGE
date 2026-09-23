using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Financeiro.Query.GetResumo;

public class GetFinanceiroResumoValidator : AbstractValidator<GetFinanceiroResumoQuery>
{
    public GetFinanceiroResumoValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("ID de empresa inválido.")
            .DeveExistirEmpresa(empresaRepository);
            
    }
}
