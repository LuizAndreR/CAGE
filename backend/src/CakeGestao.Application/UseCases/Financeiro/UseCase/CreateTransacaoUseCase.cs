using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Transacao;
using CakeGestao.Application.UseCases.Financeiro.Interface;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Financeiro.UseCase;

public class CreateTransacaoUseCase : ICreateTransacaoUseCase
{
    private readonly IFinanceiroRepository _financeiroRepository;
    private readonly ILogger<CreateTransacaoUseCase> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTransacaoRequest> _validator;

    public CreateTransacaoUseCase(IFinanceiroRepository financeiroRepository, ILogger<CreateTransacaoUseCase> logger, IMapper mapper, IValidator<CreateTransacaoRequest> validator)
    {
        _financeiroRepository = financeiroRepository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(CreateTransacaoRequest request, int empresaId, int? pedidoId)
    {
        _logger.LogInformation("Iniciando criação de nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);

        _logger.LogInformation("Validando dados da requisição nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("A valirificaçao dos dodos requisição falou nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Fail(new ValidationError(errors));
        }
        _logger.LogInformation("Dados validados com sucesso para nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);

        _logger.LogInformation("Validadndo tipo de transação da requisição nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);
        if (!Enum.TryParse<TipoTransacaoEnum>(request.Tipo, true, out var tipoTransacao))
        {
            _logger.LogInformation("Tipo de transação inválido na requisição nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);
            return Result.Fail(new ValidationError(new List<string> { $"Tipo de transação inválido: {request.Tipo}" }));
        }
        _logger.LogInformation("Tipo de transação validado com sucesso para nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);

        _logger.LogInformation("Validando categoria da requisição nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);
        if (!Enum.TryParse<CategoriasEnum>(request.Categoria, true, out var categoria))
        {
            _logger.LogInformation("Categoria inválida na requisição nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);
            return Result.Fail(new ValidationError(new List<string> { $"Categoria inválida: {request.Categoria}" }));
        }
        _logger.LogInformation("Categoria validada com sucesso para nova transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);

        //Add varrificaçao de pedidoId se existe e EmpreaId se existe

        _logger.LogInformation("Mapeando dados da requisição para entidade de transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);
        var transacaoEntity = _mapper.Map<TransacaoFinanceira>(request);
        transacaoEntity.PedidoId = pedidoId ?? 0;
        transacaoEntity.EmpresaId = empresaId;
        _logger.LogInformation("Mapeamento concluído com sucesso para transação financeira do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);

        _logger.LogInformation("Adicionando ID do pedido à transação financeira, se aplicável, para transação do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);
        await _financeiroRepository.CreateTransacaoAsync(transacaoEntity);
        _logger.LogInformation("Adicionando transação financeira ao repositório concluída com sucesso para transação do tipo {Tipo} com valor {Valor}", request.Tipo, request.Valor);

        _logger.LogInformation("Processo de Create nova transacao realizada com susseso");
        return Result.Ok();
    }
}
