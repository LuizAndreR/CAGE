using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Pedidos.Query.Get;

public class GetPedidoByIdValidator : AbstractValidator<GetPedidoByIdQuery>
{
    public GetPedidoByIdValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O ID do pedido deve ser um número positivo.");
    }
}