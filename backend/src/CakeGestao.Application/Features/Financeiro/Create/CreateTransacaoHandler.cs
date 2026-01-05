using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Financeiro.Create;

public class CreateTransacaoHandler : IRequestHandler<CreateTransacaoCommand, Result>
{
    private readonly IFinanceiroRepository _financeiroRepository;
    private readonly ILogger<CreateTransacaoHandler> _logger;
    private readonly IValidator<CreateTransacaoCommand> _validator;
    private const string UseCaseLogPrefix = "[Create Transacao]";

    public CreateTransacaoHandler(IFinanceiroRepository financeiroRepository, ILogger<CreateTransacaoHandler> logger, IValidator<CreateTransacaoCommand> validator)
    {
        _financeiroRepository = financeiroRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result> Handle(CreateTransacaoCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo. Tipo: {Tipo}, Valor: {Valor}, EmpresaId: {EmpresaId}", UseCaseLogPrefix, request.Tipo, request.Valor, request.EmpresaId);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou. Erros: {Errors}", UseCaseLogPrefix, errors);
            return Result.Fail(new ValidationError(errors));
        }

        var tipoEnum = Enum.Parse<TipoTransacaoEnum>(request.Tipo);
        var categoriaEnum = Enum.Parse<CategoriasEnum>(request.Categoria);
        var transacaoEntity = new TransacaoFinanceira
        (
            tipo: tipoEnum,
            categoria: categoriaEnum,
            valor: request.Valor,
            data: request.Data,
            pedidoId: request.PedidoId,
            empresaId: request.EmpresaId
        );

        await _financeiroRepository.CreateTransacaoAsync(transacaoEntity);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência concluída com sucesso para transação. EmpresaId: {EmpresaId}, PedidoId: {PedidoId}, Valor: {Valor}", UseCaseLogPrefix, request.EmpresaId, transacaoEntity.PedidoId, transacaoEntity.Valor);

        return Result.Ok();
    }
}