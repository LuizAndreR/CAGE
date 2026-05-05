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
    private const string LogPrefix = "[Create Receita Handler]";
    
    public CreatePedidoHandler(ILogger<CreatePedidoHandler> logger, IValidator<CreatePedidoCommand> validator, IReceitaRepository receitaRepository, IPedidoRepository pedidoRepository)
    {
        _logger = logger;
        _validator = validator;
        _receitaRepository = receitaRepository;
        _pedidoRepository = pedidoRepository;
    }
    
    public async Task<Result> Handle(CreatePedidoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("");
        
        var validatorResult = await _validator.ValidateAsync(request);
        if (!validatorResult.IsValid)
        {
            var errors = validatorResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou. Nome: {Nome}. Erros: {Errors}", LogPrefix, request.ClienteNome, string.Join(", ", errors));
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
                _logger.LogWarning("{LogPrefix} Produto/Receita ID {ReceitaId} não encontrado.", LogPrefix, dto.ReceitaId);
                return Result.Fail(new NotFoundError($"Receita de id: {dto.ReceitaId} não encontrado."));
            }

            var receita = receitaResult.Value;

            var itemPedido = new ItemPedido(receita.Id, dto.Quantidade, receita.PrecoVenda);
            
            pedido.AdicionarItem(itemPedido);
        }
        _logger.LogInformation("{LogPrefix} Itens processados. Valor Total do Pedido: {ValorTotal}", LogPrefix, pedido.ValorTotal);
        
        await _pedidoRepository.CreatePedidoAsync(pedido);
        _logger.LogInformation("{LogPrefix} Pedido criado com sucesso! ID: {PedidoId}", LogPrefix, pedido.Id);

        return Result.Ok();
    }
}