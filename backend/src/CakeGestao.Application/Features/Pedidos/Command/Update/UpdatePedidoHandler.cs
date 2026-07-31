using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Pedidos.Command.Update;

public class UpdatePedidoHandler : IRequestHandler<UpdatePedidoCommand, Result>
{
    private readonly IValidator<UpdatePedidoCommand> _validator;
    private readonly ILogger<UpdatePedidoHandler> _logger;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IReceitaRepository _receitaRepository;
    private const string LogPrefix = "[Update Pedido Handler]";
    
    public UpdatePedidoHandler(IValidator<UpdatePedidoCommand> validator, ILogger<UpdatePedidoHandler> logger, IPedidoRepository pedidoRepository, IReceitaRepository receitaRepository)
    {
        _validator = validator;
        _logger = logger;
        _pedidoRepository = pedidoRepository;
        _receitaRepository = receitaRepository;
    }

    public async Task<Result> Handle(UpdatePedidoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando atualização do pedido {PedidoId} da empresa {EmpresaId}", LogPrefix, request.Id, request.EmpresaId);
        
        var validatorResult = await _validator.ValidateAsync(request);
        if (!validatorResult.IsValid)
        {
            var errors = validatorResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou. Pedido: {PedidoId}. Erros: {Errors}", LogPrefix, request.Id, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }      
        
        var pedidoResult = await _pedidoRepository.GetByIdAsync(request.Id, request.EmpresaId);
        if (pedidoResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Pedido {PedidoId} não encontrado.", LogPrefix, request.Id);
            return Result.Fail(new NotFoundError("Pedido não encontrado."));
        }
        Pedido pedido = pedidoResult.Value;
        pedido.Atualizar(request.ClienteNome, request.TelefoneCliente, request.Descricao, request.DataEntrega);

        bool itensMudados = VerificarSeItensMudaram(pedido.Itens, request.Itens);

        if (itensMudados)
        {
            _logger.LogInformation("{LogPrefix} Os itens foram alterados. Recalculando carrinho e reiniciando pagamento.", LogPrefix);
            
            pedido.LimparItens();
            
            foreach (var itemDto in request.Itens)
            {
                var receitarResult = await _receitaRepository.GetReceitaByIdAsync(itemDto.ReceitaId, request.EmpresaId);
                
                if (receitarResult.IsFailed)
                    return Result.Fail(new NotFoundError( $"A receita com ID {itemDto.ReceitaId} não foi encontrada."));
                Receita receita = receitarResult.Value;
                
                ItemPedido item = new (receita.Id, itemDto.Quantidade, receita.PrecoVenda); 
                pedido.AdicionarItem(item);
            }
        }
        
        await _pedidoRepository.UpdatePedidoAsync(pedido); 

        return Result.Ok();
    }

    private bool VerificarSeItensMudaram(ICollection<ItemPedido> itensBanco,
        List<UpdatePedidoItemDto> itensRequest)
    {
        
        if (itensBanco.Count != itensRequest.Count)
        {
            return true;
        }

        foreach (var itemDto in itensRequest)
        {
            var itemExiste = itensBanco.Any(i => i.ReceitaId == itemDto.ReceitaId && i.Quantidade == itemDto.Quantidade);
            
            if (!itemExiste)
                return true;
        }
        
        return false;
    }
    
}