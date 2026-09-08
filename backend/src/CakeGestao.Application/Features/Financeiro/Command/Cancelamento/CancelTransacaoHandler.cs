using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Financeiro.Command.Cancelamento;

public class CancelTransacaoHandler : IRequestHandler<CancelTransacaoCommand, Result>
{
    private readonly IFinanceiroRepository _repository;
    private readonly IValidator<CancelTransacaoCommand> _validator;
    private readonly ILogger<CancelTransacaoHandler> _logger;
    private const string LogPrefix = "[Cancel Transacao Handler]";

    public CancelTransacaoHandler(IFinanceiroRepository repository, IValidator<CancelTransacaoCommand> validator, ILogger<CancelTransacaoHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelTransacaoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando o cancelamento da transação. TransacaoId: {TransacaoId}, EmpresaId: {EmpresaId}, UsuarioId: {UsuarioId}", LogPrefix, request.TransacaoId, request.EmpresaId, request.UsuarioId);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            List<string> errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou para o cancelamento da transação. Erros: {Errors}", LogPrefix, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }

        var transacaoResult = await _repository.GetTransacaoAsync(request.TransacaoId, request.EmpresaId);
        if (transacaoResult.IsFailed)
        {
            string erro = transacaoResult.Errors.FirstOrDefault()!.Message;
            _logger.LogWarning("{LogPrefix} Transação não encontrada. TransacaoId: {TransacaoId}, EmpresaId: {EmpresaId}", LogPrefix, request.TransacaoId, request.EmpresaId);
            return Result.Fail(new NotFoundError(erro));
        }
        TransacaoFinanceira transacao = transacaoResult.Value;

        transacao.Cancelar(request.Motivo, request.UsuarioId);

        await _repository.UpdateTransacaoAsync(transacao);

        _logger.LogInformation("{LogPrefix} Cancelamento da transação concluído com sucesso. TransacaoId: {TransacaoId}", LogPrefix, request.TransacaoId);
        return Result.Ok();
    }
}
