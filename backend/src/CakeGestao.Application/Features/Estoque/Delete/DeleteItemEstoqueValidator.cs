using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.UseCases.Estoque.Delete;

public class DeleteItemEstoqueValidator : AbstractValidator<DeleteItemEstoqueCommand>
{
    public DeleteItemEstoqueValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
    }
}
