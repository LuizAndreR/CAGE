using CakeGestao.Application.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Pedidos.Command.UpdateStatus;

public class UpdateStatusPedidoValidator : AbstractValidator<UpdateStatusPedidoCommand>
{
    public UpdateStatusPedidoValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.EmpresaId)
            .DeveExistirEmpresa(empresaRepository);
        
        RuleFor(x => x.Status)
            .IsEnumName(typeof(StatusPedidoEnum), caseSensitive: false).WithMessage($"Categoria inválida. Valores aceitos: {string.Join(", ", Enum.GetNames(typeof(StatusPedidoEnum)))}");
    }
}