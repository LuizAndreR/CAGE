using AutoMapper;
using CakeGestao.Application.Features.Financeiro.Common;
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
    private const string UseCaseLogPrefix = "[Get Transacao]";
    
    public GetTransacaoHandler(IFinanceiroRepository repository, IMapper mapper, IValidator<GetTransacaoQuery> validator, ILogger<GetTransacaoHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<TransacaoResponse>> Handle(GetTransacaoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix}Iniciando o processo de busca da transação de id: {Id}", UseCaseLogPrefix, request.Id);
        
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var erros = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação dos dados da busca da transação de id: {Id} falhou. Erros: {Errors}", UseCaseLogPrefix, request.Id, erros);
            return Result.Fail(new ValidationError(erros));
        }
        
        var transacaoResult = await _repository.GetTransacaoAsync(request.Id);
        if (transacaoResult.IsFailed)
        {
            _logger.LogWarning("Transação de id: {Id} não encontrado no banco de dados", request.Id);
            return Result.Fail(new NotFoundError("Trnsação não encontrado"));
        }
        
        var transacao = _mapper.Map<TransacaoResponse>(transacaoResult.Value);
        
        _logger.LogInformation("{UseCaseLogPrefix} Processo de busca da transação de id: {Id} realizado com susseso", UseCaseLogPrefix, request.Id);
        return Result.Ok(transacao);
    }
}