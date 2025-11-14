using CakeGestao.Application.Dtos.Requests.Transacao;
using FluentValidation;

namespace CakeGestao.Application.Validators.Financeiro;

public class CreateTransacaoValidator : AbstractValidator<CreateTransacaoRequest>
{
    public CreateTransacaoValidator()
    {
        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("O tipo da transação é obrigatório.")
            .MaximumLength(50).WithMessage("O tipo da transação não pode exceder 50 caracteres.");

        RuleFor(x => x.Categoria)
            .NotEmpty().WithMessage("A Categoria da transação é obrigatório.")
            .MaximumLength(50).WithMessage("A Categoria da transação não pode exceder 50 caracteres.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("O valor da transação deve ser maior que zero.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("A data da transação é obrigatória.");

        RuleFor(x => x.Descricao)
            .MaximumLength(1000).WithMessage("A descrição da transação não pode exceder 1000 caracteres.");
    }
}
