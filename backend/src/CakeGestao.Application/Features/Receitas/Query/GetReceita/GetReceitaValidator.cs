using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Receitas.Query.GetReceita;

public class GetReceitaValidator : AbstractValidator<GetReceitaQuery>
{
    public GetReceitaValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O ID da receita deve ser maior que zero.");

        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
    }
}
