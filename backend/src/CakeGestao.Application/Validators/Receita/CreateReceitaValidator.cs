using CakeGestao.Application.Dtos.Requests.Receita;
using FluentValidation;

namespace CakeGestao.Application.Validators.Receita;

public class CreateReceitaValidator : AbstractValidator<CreateReceitaRequest>
{
    public CreateReceitaValidator()
    {
        RuleFor(r => r.Nome)
            .NotEmpty().WithMessage("O nome e obrigatório");
        
        RuleFor(r => r.ModePreparo)
            .NotEmpty().WithMessage("O Modo de preparo é obrigatório");
        
        RuleFor(r => r.Preco)
            .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser um valor negativo.");
    }
}