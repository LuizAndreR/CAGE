using AutoMapper;
using CakeGestao.Application.Features.Financeiro.Common;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Financeiro.Query.Get;

public class GetTransacaoHandler : IRequestHandler<GetTransacaoQuery, Result<TransacaoResponse>>
{
    private readonly IFinanceiroRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetTransacaoQuery> _validator;
    private readonly ILogger<GetTransacaoHandler> _logger;
    private const string LogPrefix = "[Get Transacao Handler]";

    public GetTransacaoHandler(IFinanceiroRepository repository, IMapper mapper, IValidator<GetTransacaoQuery> validator, ILogger<GetTransacaoHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<TransacaoResponse>> Handle(GetTransacaoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Buscando detalhes da transação. ID: {Id} | EmpresaID: {EmpresaId}", LogPrefix, request.Id, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var erros = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Requisição inválida. ID: {Id}. Erros: {Errors}", LogPrefix, request.Id, string.Join(", ", erros));
            return Result.Fail(new ValidationError(erros));
        }

        var transacaoResult = await _repository.GetTransacaoAsync(request.Id, request.EmpresaId);
        if (transacaoResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Transação não encontrada no banco. ID: {Id}", LogPrefix, request.Id);
            var erro = transacaoResult.Errors.ToString();
            return Result.Fail(new NotFoundError(erro!));
        }

        var transacao = _mapper.Map<TransacaoResponse>(transacaoResult.Value);

        _logger.LogInformation("{LogPrefix} Dados retornados com sucesso. ID: {Id}", LogPrefix, request.Id);
        return Result.Ok(transacao);
    }
}