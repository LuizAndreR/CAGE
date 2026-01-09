using AutoMapper;
using CakeGestao.Application.Features.Financeiro.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Financeiro.Query.GetAll;

public class GetAllTransacaoHandler : IRequestHandler<GetAllTransacaoQuery, Result<List<TransacaoResponse>>>
{
    private readonly IFinanceiroRepository _financeiroRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllTransacaoHandler> _logger;
    private readonly IValidator<GetAllTransacaoQuery> _validator;
    private const string UseCaseLogPrefix = "[GetAll Transacao]";

    public GetAllTransacaoHandler(IFinanceiroRepository financeiroRepository, IMapper mapper, ILogger<GetAllTransacaoHandler> logger, IValidator<GetAllTransacaoQuery> validator)
    {
        _financeiroRepository = financeiroRepository;
        _mapper = mapper;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<List<TransacaoResponse>>> Handle(GetAllTransacaoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando o processo de busca de todas transações da empresa de id: {Id}", UseCaseLogPrefix, request.EmpresaId);
        
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var erros =  validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            return Result.Fail(new ValidationError(erros));
        }
        
        var listTransacaoResult = await _financeiroRepository.GetAllTransacoesAsync(request.EmpresaId);
        if (listTransacaoResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Processo de busca de transações da empresa de id: {Id} falhou", UseCaseLogPrefix, request.EmpresaId);
            var erro = listTransacaoResult.Errors.ToString();
            return Result.Fail(new NotFoundError(erro!)); 
        }
        
        var listTransacao = _mapper.Map<List<TransacaoResponse>>(listTransacaoResult.Value);
        
        _logger.LogInformation("{UseCaseLogPrefix} Processo de busca de todas transações da empresa de id: {Id} realizada com susseso",  UseCaseLogPrefix, request.EmpresaId);
        return Result.Ok(listTransacao);
    }
}