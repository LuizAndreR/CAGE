using CakeGestao.Application.Dtos.Requests.Receita;
using FluentValidation;

namespace CakeGestao.Application.Validators.Receita;

public class CreateReceitaValidator : AbstractValidator<CreateReceitaRequest>
{
    public CreateReceitaValidator()
    {
        RuleFor(r => r.Nome)
            .NotEmpty().WithMessage("O nome e obrigatório");
        
        RuleFor(r => r.ModoPreparo)
            .NotEmpty().WithMessage("O Modo de preparo é obrigatório");
        
        RuleFor(r => r.PrecoVenda)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O preço de venda é obrigatório.")
            .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser um valor negativo.");
    }
}