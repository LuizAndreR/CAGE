using CakeGestao.Application.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Estoque.Command.Create;

public class CreateEstoqueValidator : AbstractValidator<CreateEstoqueCommand>
{
    public CreateEstoqueValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome do estoque é obrigatório.")
            .MaximumLength(100).WithMessage("O nome do estoque deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("A marca do estoque é obrigatória.")
            .MaximumLength(50).WithMessage("A marca do estoque deve ter no máximo 50 caracteres.");

        RuleFor(x => x.QuantidadeAtual)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade atual não pode ser negativa.");
        
        RuleFor(x => x.QuantidadeMinima)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade mínima não pode ser negativa.");

        RuleFor(x => x.UnidadeMedida)
            .IsEnumName(typeof(UnidadeMedidaEnum), caseSensitive: false)
            .WithMessage($"Tipo de unidade de medida inválida. Valores permitidos: {string.Join(", ", Enum.GetNames(typeof(UnidadeMedidaEnum)))}.");

        RuleFor(x => x.UnidadeMedidaReferenciaVolume)
            .IsEnumName(typeof(UnidadeMedidaEnum), caseSensitive: false)
            .When(x => !string.IsNullOrWhiteSpace(x.UnidadeMedidaReferenciaVolume))
            .WithMessage($"Unidade de referência inválida. Valores permitidos: {string.Join(", ", Enum.GetNames(typeof(UnidadeMedidaEnum)))}.");

        RuleFor(x => x.PesoReferenciaEmGramas)
            .GreaterThan(0).WithMessage("O peso de referência deve ser maior que zero (gramas).")
            .When(x => x.PesoReferenciaEmGramas.HasValue);

        RuleFor(x => x)
            .Must(x =>
                (string.IsNullOrWhiteSpace(x.UnidadeMedidaReferenciaVolume) && !x.PesoReferenciaEmGramas.HasValue) ||
                (!string.IsNullOrWhiteSpace(x.UnidadeMedidaReferenciaVolume) && x.PesoReferenciaEmGramas.HasValue))
            .WithMessage("Você deve informar tanto a Unidade de Referência quanto o Peso, ou deixar ambos em branco.");
    }
}
