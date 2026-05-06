using AutoMapper;
using CakeGestao.Application.Features.Pedidos.Common;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Pedidos.Query.Get;

public class GetPedidoByIdHandler : IRequestHandler<GetPedidoByIdQuery, Result<GetPedidoResponse>>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IValidator<GetPedidoByIdQuery> _validator;
    private readonly ILogger<GetPedidoByIdHandler> _logger;
    private readonly IMapper _mapper;
    private const string LogPrefix = "[Get Pedido By Id Handler]";

    public GetPedidoByIdHandler(IPedidoRepository pedidoRepository, IValidator<GetPedidoByIdQuery> validator, ILogger<GetPedidoByIdHandler> logger, IMapper mapper)
    {
        _pedidoRepository = pedidoRepository;
        _validator = validator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<GetPedidoResponse>> Handle(GetPedidoByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Buscando detalhes do pedido. ID: {Id} | EmpresaId: {EmpresaId}", LogPrefix, request.Id, request.EmpresaId);
        
        var validatorResult = await _validator.ValidateAsync(request);
        if (!validatorResult.IsValid)
        {
            var errors = validatorResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Pedido ID {Id} não encontrado ou acesso negado para a EmpresaId {EmpresaId}.", LogPrefix, request.Id, request.EmpresaId);
            return Result.Fail(new ValidationError(errors));
        } 
        
        var pedidoResult = await _pedidoRepository.GetByIdAsync(request.Id, request.EmpresaId);

        if (pedidoResult.IsFailed)
        {  
            _logger.LogWarning("{LogPrefix} Pedido de id: {Id} não encontrado", LogPrefix, request.Id);
            string erro = pedidoResult.Errors.Select(e => e.Message.ToString()).FirstOrDefault()!;
            return Result.Fail(new NotFoundError(erro));
        }
        
        var response = _mapper.Map<GetPedidoResponse>(pedidoResult.Value);
        
        return Result.Ok(response);
    }
}