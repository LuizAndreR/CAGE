using CakeGestao.Application.Common;
using CakeGestao.Application.Dtos.Requests.Transacao;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Validators.Financeiro;

public class CreateTransacaoValidator : AbstractValidator<CreateTransacaoRequest>
{
    public CreateTransacaoValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);

        RuleFor(x => x.Tipo)
            .IsEnumName(typeof(TipoTransacaoEnum), caseSensitive: false).WithMessage($"Tipo de transação inválido. Valores aceitos: {string.Join(", ", Enum.GetNames(typeof(TipoTransacaoEnum)))}");

        RuleFor(x => x.Categoria)
            .IsEnumName(typeof(CategoriasEnum), caseSensitive: false).WithMessage("Categoria inválida.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("O valor da transação deve ser maior que zero.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("A data da transação é obrigatória.");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000).WithMessage("A descrição da transação não pode exceder 1000 caracteres.");
    }
}
