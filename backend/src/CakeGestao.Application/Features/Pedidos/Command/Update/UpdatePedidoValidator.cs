using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using CakeGestao.Infrastructure.Data.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Pedidos.Command.Update;

public class UpdatePedidoValidator : AbstractValidator<UpdatePedidoCommand>
{
    public UpdatePedidoValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID do pedido inválido.");

        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
        
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