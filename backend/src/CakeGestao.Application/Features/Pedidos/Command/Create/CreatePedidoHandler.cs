using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Pedidos.Command.Create;

public class CreatePedidoHandler : IRequestHandler<CreatePedidoCommand, Result>
{
    private readonly ILogger<CreatePedidoHandler> _logger;
    private readonly IValidator<CreatePedidoCommand> _validator;
    private readonly IReceitaRepository _receitaRepository;
    private readonly IPedidoRepository _pedidoRepository;
    private const string LogPrefix = "[Create Pedido Handler]";
    
    public CreatePedidoHandler(ILogger<CreatePedidoHandler> logger, IValidator<CreatePedidoCommand> validator, IReceitaRepository receitaRepository, IPedidoRepository pedidoRepository)
    {
        _logger = logger;
        _validator = validator;
        _receitaRepository = receitaRepository;
        _pedidoRepository = pedidoRepository;
    }
    
    public async Task<Result> Handle(CreatePedidoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando criação de pedido. Cliente: {ClienteNome} | EmpresaId: {EmpresaId}", LogPrefix, request.ClienteNome, request.EmpresaId);
        
        var validatorResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validatorResult.IsValid)
        {
            var errors = validatorResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação estrutural falhou. Cliente: {ClienteNome}. Erros: {Errors}", LogPrefix, request.ClienteNome, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }
        
        Pedido pedido = new (
            clienteNome: request.ClienteNome,
            telefoneCliente: request.TelefoneCliente,
            descricao: request.Descricao,
            dataEntrega: request.DataEntrega,
            empresaId: request.EmpresaId
        );
        
        foreach (var dto in request.Itens)
        {
            var receitaResult = await _receitaRepository.GetReceitaByIdAsync(dto.ReceitaId, request.EmpresaId);
            
            if (receitaResult.IsFailed)
            {
                _logger.LogWarning("{LogPrefix} Receita ID {ReceitaId} não encontrada ou não pertence à empresa {EmpresaId}.", LogPrefix, dto.ReceitaId, request.EmpresaId);
                return Result.Fail(new NotFoundError($"Receita de id: {dto.ReceitaId} não encontrada."));
            }

            var receita = receitaResult.Value;

            if (!receita.Status)
            {
                _logger.LogWarning(
                    "{LogPrefix} Bloqueio: Tentativa de adicionar receita inativa. Receita ID: {ReceitaId}", LogPrefix,
                    dto.ReceitaId);
                return Result.Fail(new ValidationError($"O pedido não pode ser criado pois a receita '{receita.Nome}' está desativada no cardápio."));
            }
             
            var itemPedido = new ItemPedido(receita.Id, dto.Quantidade, receita.PrecoVenda);
            pedido.AdicionarItem(itemPedido);
        }

        _logger.LogInformation("{LogPrefix} Itens processados. Total do Pedido calculado: {ValorTotal}", LogPrefix, pedido.ValorTotal);
        
        await _pedidoRepository.CreatePedidoAsync(pedido);
            
        _logger.LogInformation("{LogPrefix} Pedido criado com sucesso. ID: {PedidoId}", LogPrefix, pedido.Id);

        return Result.Ok();
    }
}