using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Pedidos.Command.Create;

public class CreatePedidoValidator : AbstractValidator<CreatePedidoCommand>
{
    public CreatePedidoValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);

        RuleFor(x => x.ClienteNome)
            .NotEmpty().WithMessage("O nome do cliente é obrigatório.");

        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("O pedido deve conter pelo menos um item.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.ReceitaId)
                .GreaterThan(0).WithMessage("ID da receita (produto) inválido.");

            item.RuleFor(i => i.Quantidade) 
                .GreaterThan(0).WithMessage("A quantidade do produto deve ser maior que zero.");
        });
    }
}