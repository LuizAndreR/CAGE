using CakeGestao.Application.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Receitas.Command.Update;

public class UpdateReceitaValidator : AbstractValidator<UpdateReceitaCommand>
{
    public UpdateReceitaValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
           .DeveExistirEmpresa(empresaRepository);

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID da receita inválido.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome da receita é obrigatório.");

        RuleFor(x => x.ModoPreparo)
            .NotEmpty().WithMessage("O modo de preparo é obrigatório.");

        RuleFor(x => x.PrecoVenda)
            .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser um valor negativo.");

        RuleFor(x => x.PercentualCustoExtra)
           .GreaterThanOrEqualTo(0).WithMessage("O percentual de custo extra não pode ser negativo.");

        RuleFor(x => x.PercentualMargemLucro)
            .GreaterThanOrEqualTo(0).WithMessage("O percentual de margem de lucro não pode ser negativo.");

        RuleForEach(x => x.Ingredientes).ChildRules(ingrediente =>
        {
            ingrediente.RuleFor(i => i.ItemId)
                .GreaterThan(0).WithMessage("ID do item de estoque inválido.");

            ingrediente.RuleFor(i => i.Quantidade)
                .GreaterThan(0).WithMessage("A quantidade do ingrediente deve ser maior que zero.");

            ingrediente.RuleFor(i => i.UnidadeMedida)
                .IsEnumName(typeof(UnidadeMedidaEnum), caseSensitive: false)
                .WithMessage("Unidade de medida inválida.");
        })
        .When(x => x.Ingredientes != null && x.Ingredientes.Any());
    }
}
