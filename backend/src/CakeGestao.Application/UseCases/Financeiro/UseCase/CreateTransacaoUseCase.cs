using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Transacao;
using CakeGestao.Application.UseCases.Financeiro.Interface;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CakeGestao.Application.UseCases.Financeiro.UseCase;

public class CreateTransacaoUseCase : ICreateTransacaoUseCase
{
    private readonly IFinanceiroRepository _financeiroRepository;
    private readonly ILogger<CreateTransacaoUseCase> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTransacaoRequest> _validator;
    private const string UseCaseLogPrefix = "[Create Transacao]";

    public CreateTransacaoUseCase(IFinanceiroRepository financeiroRepository, ILogger<CreateTransacaoUseCase> logger, IMapper mapper, IValidator<CreateTransacaoRequest> validator)
    {
        _financeiroRepository = financeiroRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
    }

    //Remover o pedidoId e deixa somente na request como foi feito na empresaId
    public async Task<Result> ExecuteAsync(CreateTransacaoRequest request, int? pedidoId)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo. Tipo: {Tipo}, Valor: {Valor}, EmpresaId: {EmpresaId}, PedidoId: {PedidoId}", UseCaseLogPrefix, request.Tipo, request.Valor, request.EmpresaId, pedidoId);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando validação da requisição", UseCaseLogPrefix);
        ValidationResult validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{UseCaseLogPrefix} Validação falhou. Erros: {Errors}", UseCaseLogPrefix, errors);
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação concluída com sucesso", UseCaseLogPrefix);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento da requisição para entidade TransacaoFinanceira", UseCaseLogPrefix);
        var transacaoEntity = _mapper.Map<TransacaoFinanceira>(request);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído. PedidoId: {PedidoId}, Tipo: {Tipo}, Categoria: {Categoria}, Valor: {Valor}", UseCaseLogPrefix, transacaoEntity.PedidoId, transacaoEntity.Tipo, transacaoEntity.Categoria, transacaoEntity.Valor);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando persistência da transação no repositório", UseCaseLogPrefix);
        await _financeiroRepository.CreateTransacaoAsync(transacaoEntity);
        _logger.LogInformation("{UseCaseLogPrefix} Persistência concluída com sucesso para transação. EmpresaId: {EmpresaId}, PedidoId: {PedidoId}, Valor: {Valor}", UseCaseLogPrefix, request.EmpresaId, transacaoEntity.PedidoId, transacaoEntity.Valor);

        _logger.LogInformation("{UseCaseLogPrefix} Processo finalizado com sucesso", UseCaseLogPrefix);
        return Result.Ok();
    }
}