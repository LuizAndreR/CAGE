using AutoMapper;
using CakeGestao.Application.Features.Financeiro.Common;
using CakeGestao.Domain.Enum;
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
    private const string LogPrefix = "[Get All Transacao Handler]";

    public GetAllTransacaoHandler(IFinanceiroRepository financeiroRepository, IMapper mapper, ILogger<GetAllTransacaoHandler> logger, IValidator<GetAllTransacaoQuery> validator)
    {
        _financeiroRepository = financeiroRepository;
        _mapper = mapper;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<List<TransacaoResponse>>> Handle(GetAllTransacaoQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Iniciando listagem financeira. EmpresaId: {EmpresaId}", LogPrefix, request.EmpresaId);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var erros =  validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Requisição inválida. EmpresaId: {EmpresaId}. Erros: {Errors}", LogPrefix, request.EmpresaId, string.Join(", ", erros));
            return Result.Fail(new ValidationError(erros));
        }
        
        var tipoEnum = Enum.Parse<TipoTransacaoEnum>(request.Tipo);
        var categoriaEnum = Enum.Parse<CategoriasEnum>(request.Categoria);
        
        var listTransacaoResult = await _financeiroRepository.GetAllTransacoesAsync(request.EmpresaId,  tipoEnum, categoriaEnum, request.Mes, request.Ano);
        if (listTransacaoResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Falha ao buscar lista de transações.", LogPrefix);
            var erro = listTransacaoResult.Errors.ToString();
            return Result.Fail(new NotFoundError(erro!)); 
        }
        
        var listTransacao = _mapper.Map<List<TransacaoResponse>>(listTransacaoResult.Value);

        _logger.LogInformation("{LogPrefix} Listagem financeira concluída. Total de registros: {Count}", LogPrefix, listTransacao.Count);
        return Result.Ok(listTransacao);
    }
}