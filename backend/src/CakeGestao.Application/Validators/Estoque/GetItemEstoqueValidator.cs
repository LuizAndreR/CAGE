using CakeGestao.Application.Common;
using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Validators.Estoque;

public class GetItemEstoqueValidator : AbstractValidator<ItemEstoqueRequest>
{
    public GetItemEstoqueValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
    }
}
