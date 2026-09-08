using CakeGestao.Application.Features.Financeiro.Common;
using CakeGestao.Domain.Enum; 
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Financeiro.Query.GetResumo;

public class GetFinanceiroResumoHandler : IRequestHandler<GetFinanceiroResumoQuery, Result<TransacaoResumo>>
{
    private readonly ILogger<GetFinanceiroResumoHandler> _logger;
    private readonly IFinanceiroRepository _repository;
    private readonly IValidator<GetFinanceiroResumoQuery> _validator;
    private const string LogPrefix = "[Get Financeiro Handler]";

    public GetFinanceiroResumoHandler(ILogger<GetFinanceiroResumoHandler> logger, IFinanceiroRepository repository, IValidator<GetFinanceiroResumoQuery> validator)
    {
        _logger = logger;
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<TransacaoResumo>> Handle(GetFinanceiroResumoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando consulta de resumo financeiro do MÊS ATUAL para a empresa {EmpresaId}", LogPrefix, request.EmpresaId);

        // 1. Validação Estrutural
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            List<string> erros = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou: {Errors}", LogPrefix, string.Join(", ", erros));
            return Result.Fail(new ValidationError(erros));
        }

        var totalEntrada = await _repository.GetTotalPorTipoMesAtualAsync(TipoTransacaoEnum.Entrada, request.EmpresaId, cancellationToken);
        var totalSaida = await _repository.GetTotalPorTipoMesAtualAsync(TipoTransacaoEnum.Saida, request.EmpresaId, cancellationToken);

        // 3. Monta o DTO de resposta
        var transacaoSaldo = new TransacaoResumo 
        { 
            Entrada = totalEntrada, 
            Saida = totalSaida 
        };

        _logger.LogInformation("{LogPrefix} Consulta concluída. Entradas: {Entrada} | Saídas: {Saida}", LogPrefix, totalEntrada, totalSaida);
        
        return Result.Ok(transacaoSaldo);
    }
}