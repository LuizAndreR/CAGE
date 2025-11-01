using CakeGestao.Application.Dtos.Requests.Receita;
using FluentValidation;

namespace CakeGestao.Application.Validators.Receita;

public class UpdateReceitaValidator : AbstractValidator<UpdateReceitaRequest>
{
    public UpdateReceitaValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome da receita é obrigatório.")
            .MaximumLength(100).WithMessage("O nome da receita não pode exceder 100 caracteres.");

        RuleFor(x => x.ModoPreparo)
            .NotEmpty().WithMessage("O modo de preparo é obrigatório.");

        RuleFor(x => x.PrecoVenda)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O preço de venda é obrigatório.")
            .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser um valor negativo.");
    }
}
