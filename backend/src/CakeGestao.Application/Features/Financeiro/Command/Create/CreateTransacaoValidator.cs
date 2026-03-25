using CakeGestao.Application.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Financeiro.Command.Create;

public class CreateTransacaoValidator : AbstractValidator<CreateTransacaoCommand>
{
    public CreateTransacaoValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);

        RuleFor(x => x.Tipo)
            .IsEnumName(typeof(TipoTransacaoEnum), caseSensitive: false).WithMessage($"Tipo de transação inválido. Valores aceitos: {string.Join(", ", Enum.GetNames(typeof(TipoTransacaoEnum)))}");

        RuleFor(x => x.Categoria)
            .IsEnumName(typeof(CategoriasEnum), caseSensitive: false).WithMessage($"Categoria inválida. Valores aceitos: {string.Join(", ", Enum.GetNames(typeof(CategoriasEnum)))}");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("O valor da transação deve ser maior que zero.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("A data da transação é obrigatória.")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("A data da transação não pode estar no futuro.");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000).WithMessage("A descrição da transação não pode exceder 1000 caracteres.");
    }
}
