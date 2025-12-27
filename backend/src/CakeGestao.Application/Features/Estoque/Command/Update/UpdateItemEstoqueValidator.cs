using CakeGestao.Application.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Estoque.Command.Update;

public class UpdateItemEstoqueValidator : AbstractValidator<UpdateItemEstoqueCommand>
{
    public UpdateItemEstoqueValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepository);

        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("O ID do item de estoque deve ser maior que zero.");

        RuleFor(x => x.Nome)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O nome do item de estoque é obrigatório.")
            .MaximumLength(100).WithMessage("O nome do item de estoque não pode exceder 100 caracteres.");

        RuleFor(x => x.QuantidadeAtual)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade do item de estoque não pode ser negativa.");

        RuleFor(x => x.UnidadeMedida)
            .IsEnumName(typeof(UnidadeMedidaEnum), caseSensitive: false).WithMessage("Tipo de unidade de medida inválida.");
    }
}
