using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.UseCases.Estoque.GetItem;

public class GetItemEstoqueValidator : AbstractValidator<GetItemEstoqueQuery>
{
    public GetItemEstoqueValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
    }
}
