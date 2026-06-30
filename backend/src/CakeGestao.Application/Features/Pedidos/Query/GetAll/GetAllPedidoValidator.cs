using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Pedidos.Query.GetAll;

public class GetAllPedidoValidator : AbstractValidator<GetAllPedidoQuery>
{
    public GetAllPedidoValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
    }
}