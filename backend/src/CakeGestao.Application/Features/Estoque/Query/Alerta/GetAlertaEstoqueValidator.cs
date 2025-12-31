using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Estoque.Query.Alerta;

public class GetAlertaEstoqueValidator : AbstractValidator<GetAlertaEstoqueQuery>
{
    public GetAlertaEstoqueValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
    }
}
