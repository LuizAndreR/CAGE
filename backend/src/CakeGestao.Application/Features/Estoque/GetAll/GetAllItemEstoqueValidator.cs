using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.UseCases.Estoque.GetAll;

public class GetAllItemEstoqueValidator : AbstractValidator<GetAllItemEstoqueQuery>
{
    public GetAllItemEstoqueValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
    }
}
