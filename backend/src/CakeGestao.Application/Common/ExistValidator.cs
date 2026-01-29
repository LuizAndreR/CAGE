using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Common;

internal static class ExistValidator
{
    public static IRuleBuilderOptions<T, int> DeveExistirEmpresa<T>(
        this IRuleBuilder<T, int> ruleBuilder, IEmpresaRepository repo)
    {
        return ruleBuilder
            .MustAsync(async (id, cancellation) =>
            {
                var result = await repo.GetEmpresaByIdAsync(id);
                return result.IsFailed;
            })
            .WithMessage("A Empresa informada não foi encontrada.");
    }

    /*
    public static IRuleBuilderOptions<T, int> DeveExistirPedido<T>(
            this IRuleBuilder<T, int> ruleBuilder,
            IPedidoRepository repo)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("O ID do Pedido é inválido.")
            .MustAsync(async (id, cancellation) =>
            {
                return await repo.ExistsAsync(id);
            })
            .WithMessage("O Pedido informado não foi encontrado.");
    }
    */
}
