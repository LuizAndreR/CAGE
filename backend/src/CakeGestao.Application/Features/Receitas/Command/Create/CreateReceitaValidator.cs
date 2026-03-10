using FluentValidation;

namespace CakeGestao.Application.Features.Receitas.Command.Create;

public class CreateReceitaValidator : AbstractValidator<CreateReceitaCommand>
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