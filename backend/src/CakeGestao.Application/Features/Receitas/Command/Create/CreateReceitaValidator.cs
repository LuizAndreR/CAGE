using CakeGestao.Application.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Receitas.Command.Create;

public class IngredienteDtoValidator : AbstractValidator<IngredienteRequestDto>
{
    public IngredienteDtoValidator()
    {
        RuleFor(i => i.ItemId)
            .GreaterThan(0).WithMessage("O ID do item de estoque é inválido.");

        RuleFor(i => i.Quantidade)
            .GreaterThan(0).WithMessage("A quantidade do ingrediente deve ser maior que zero.");

        RuleFor(i => i.UnidadeMedida)
            .IsEnumName(typeof(UnidadeMedidaEnum), caseSensitive: false)
            .WithMessage($"Tipo de unidade de medida inválida. Valores permitidos: {string.Join(", ", Enum.GetNames(typeof(UnidadeMedidaEnum)))}.");
    }
}

public class CreateReceitaValidator : AbstractValidator<CreateReceitaCommand>
{
    public CreateReceitaValidator(IEmpresaRepository empresaRepo, IEstoqueRepository estoqueRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);
        
        RuleFor(r => r.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório");
        
        RuleFor(r => r.ModoPreparo)
            .NotEmpty().WithMessage("O Modo de preparo é obrigatório");
        
        RuleFor(r => r.PrecoVenda)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser um valor negativo.");

        RuleForEach(r => r.Ingredientes)
            .SetValidator(new IngredienteDtoValidator());

        RuleForEach(r => r.Ingredientes)
            .MustAsync(async (command, ingrediente, cancellation) =>
            {
                var result = await estoqueRepo.GetItemEstoqueByIdAsync(ingrediente.ItemId, command.EmpresaId);
                return result.IsSuccess;
            })
            .WithMessage((command, ingrediente) => $"O Item de estoque informado (ID: {ingrediente.ItemId}) não foi encontrado.");
    }
}