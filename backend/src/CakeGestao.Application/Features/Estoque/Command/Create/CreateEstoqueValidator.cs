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

        RuleFor(x => x.QuantidadeAtual)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade atual não pode ser negativa.");

        RuleFor(x => x.UnidadeMedida)
            .IsEnumName(typeof(UnidadeMedidaEnum), caseSensitive: false).WithMessage("Tipo de unidade de medida inválida.");
    }
}
