using AutoMapper;
using CakeGestao.Application.Features.Pedidos.Common;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Pedidos.Query.GetAll;

public class GetAllPedidoHandler : IRequestHandler<GetAllPedidoQuery, Result<List<GetAllPedidosResponse>>>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllPedidoHandler> _logger;
    private readonly IValidator<GetAllPedidoQuery> _validator;
    private const string LogPrefix = "[Get All Pedidos Handler]";

    public GetAllPedidoHandler(IPedidoRepository pedidoRepository, IMapper mapper, ILogger<GetAllPedidoHandler> logger, IValidator<GetAllPedidoQuery> validator)
    {
        _pedidoRepository = pedidoRepository;
        _mapper = mapper;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<List<GetAllPedidosResponse>>> Handle(GetAllPedidoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando listagem de pedidos. EmpresaId: {EmpresaId}", LogPrefix, request.EmpresaId);
        
        var validatorResult = await _validator.ValidateAsync(request);
        if (!validatorResult.IsValid)
        {
            var errors = validatorResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Requisição inválida. EmpresaId: {EmpresaId}. Erros: {Errors}", LogPrefix, request.EmpresaId, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }
        
        var pedidoResult = await _pedidoRepository.GetAllByEmpresaIdAsync(request.EmpresaId);
        if (pedidoResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Não encontrado pedidos da empresa {EmpresaId} cadastrado.", LogPrefix, request.EmpresaId);
            string erro = pedidoResult.Errors.Select(e => e.Message.ToString()).FirstOrDefault()!;
            return Result.Fail(new NotFoundError(erro));
        }
        
        var response = _mapper.Map<List<GetAllPedidosResponse>>(pedidoResult.Value);
        
        _logger.LogInformation("{LogPrefix} Listagem concluída. Total de pedidos retornados: {Count}", LogPrefix, response.Count);
        return Result.Ok(response);
    }
}