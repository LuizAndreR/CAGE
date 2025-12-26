using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Infrastructure.Data.Repositories;
using FluentValidation;

namespace CakeGestao.Application.UseCases.Estoque.RemoverQuantidade;

public class RemoverQuantidadeEstoqueValidator : AbstractValidator<RemoveQuantidadeEstoqueCommand>
{
    public RemoverQuantidadeEstoqueValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepository);

        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("O ID do item de estoque deve ser maior que zero.");

        RuleFor(x => x.QuantidadeARemover)
            .GreaterThan(0).WithMessage("A quantidade a remover deve ser maior que zero.");
    }
}
