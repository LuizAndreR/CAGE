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
                return result.IsSuccess;
            })
            .WithMessage("A Empresa informada não foi encontrada.");
    }
    
    public static IRuleBuilderOptions<T, int> DeveExistirItem<T>(
        this IRuleBuilder<T, int> ruleBuilder, IEstoqueRepository itemRepo,
        Func<T, int> getEmpresaId) 
    {
        return ruleBuilder
            .MustAsync(async (command, itemId, cancellation) =>
            {
                var empresaId = getEmpresaId(command);
            
                var item = await itemRepo.GetItemEstoqueByIdAsync(itemId, empresaId);
                return item.IsSuccess;
            })
            .WithMessage("O Item de estoque informado ({PropertyValue}) não foi encontrado.");
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
