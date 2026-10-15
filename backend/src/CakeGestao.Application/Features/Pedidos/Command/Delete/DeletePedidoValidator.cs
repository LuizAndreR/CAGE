using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Pedidos.Command.Delete;

public class DeletePedidoValidator : AbstractValidator<DeletePedidoCommand>
{
    public DeletePedidoValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.PedidoId).GreaterThan(0);
        RuleFor(x => x.UsuarioId).GreaterThan(0).WithMessage("Usuário responsável pela exclusão inválido.");

        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
    }
}