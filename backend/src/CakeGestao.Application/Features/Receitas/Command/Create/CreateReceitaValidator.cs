using CakeGestao.Application.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Receitas.Command.Create;

public class CreateReceitaValidator : AbstractValidator<CreateReceitaCommand>
{
    public CreateReceitaValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);

        RuleFor(r => r.Nome)
            .NotEmpty().WithMessage("O nome da receita é obrigatório.");

        RuleFor(r => r.ModoPreparo)
            .NotEmpty().WithMessage("O modo de preparo é obrigatório.");

        RuleFor(r => r.PrecoVenda)
            .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser um valor negativo.");

        RuleFor(x => x.CustoExtra)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O valor de custo extra não pode ser negativo.");

        RuleFor(x => x.PercentualMargemLucro)
            .GreaterThanOrEqualTo(0).WithMessage("O percentual de margem de lucro não pode ser negativo.");

        RuleForEach(r => r.Ingredientes).ChildRules(ingrediente =>
        {
            ingrediente.RuleFor(i => i.ItemId)
                .GreaterThan(0).WithMessage("O ID do item de estoque é inválido.");

            ingrediente.RuleFor(i => i.Quantidade)
                .GreaterThan(0).WithMessage("A quantidade do ingrediente deve ser maior que zero.");

            ingrediente.RuleFor(i => i.UnidadeMedida)
                .IsEnumName(typeof(UnidadeMedidaEnum), caseSensitive: false)
                .WithMessage($"Tipo de unidade de medida inválida. Valores permitidos: {string.Join(", ", Enum.GetNames(typeof(UnidadeMedidaEnum)))}.");
        })
        .When(x => x.Ingredientes != null && x.Ingredientes.Any());
    }
}