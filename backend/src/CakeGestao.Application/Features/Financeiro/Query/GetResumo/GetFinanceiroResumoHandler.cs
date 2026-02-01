using AutoMapper;
using CakeGestao.Application.Features.Financeiro.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Financeiro.Query.GetEntrada;

public class GetFinanceiroResumoHandler : IRequestHandler<GetFinanceiroResumoQuery, Result<TransacaoResumo>>
{
    private readonly ILogger<GetFinanceiroResumoHandler> _logger;
    private readonly IFinanceiroRepository _repository;
    private readonly IValidator<GetFinanceiroResumoQuery> _validator;
    private const string LogPrefix = "[Get Financeiro Entrada Handler]";

    public GetFinanceiroResumoHandler(ILogger<GetFinanceiroResumoHandler> logger, IFinanceiroRepository repository,IValidator<GetFinanceiroResumoQuery> validator)
    {
        _logger = logger;
        _repository = repository;
        _validator = validator;
    }

    public async Task<Result<TransacaoResumo>> Handle(GetFinanceiroResumoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando manipulação da consulta de entrada financeira para a empresa {EmpresaId}", LogPrefix, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            List<string> erros = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou para a consulta de entrada financeira: {Errors}", LogPrefix, string.Join(", ", erros));
            return Result.Fail(new ValidationError(erros));
        }

        var entradaTotalResult = await _repository.GetEntradaAsync(request.EmpresaId);
        var saidaTotalResult = await _repository.GetSaidaAsync(request.EmpresaId);
        if (entradaTotalResult.IsFailed && saidaTotalResult.IsFailed)
        {
            string errorMessage = entradaTotalResult.Errors.FirstOrDefault()!.Message;
            _logger.LogWarning("{LogPrefix} Falha ao obter o resunmo financeira da empresaId: {EmpresaId}", LogPrefix, request.EmpresaId);
            return Result.Fail(new NotFoundError("Nenhum trasanção financeira cadastrada"));
        }

        var transacaoSaldo = new TransacaoResumo { Entrada = entradaTotalResult.Value, Saida = saidaTotalResult.Value};

        _logger.LogInformation("{LogPrefix} Consulta de entrada financeira concluída com sucesso para a empresa {EmpresaId}", LogPrefix, request.EmpresaId);
        return Result.Ok(transacaoSaldo);
    }
}
