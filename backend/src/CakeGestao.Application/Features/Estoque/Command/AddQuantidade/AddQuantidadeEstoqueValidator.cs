using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Estoque.Command.AddQuantidade;

public class AddQuantidadeEstoqueValidator : AbstractValidator<AddQuantidadeEstoqueCommand>
{
    public AddQuantidadeEstoqueValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);

        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("ID do item inválido.");

        RuleFor(x => x.QuantidadeAdicionar)
            .GreaterThan(0).WithMessage("A quantidade a ser adicionada deve ser maior que zero.");

        RuleFor(x => x.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor não pode ser negativo.");
    }
}
