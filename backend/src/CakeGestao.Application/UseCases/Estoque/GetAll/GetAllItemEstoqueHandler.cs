using AutoMapper;
using CakeGestao.Application.UseCases.Estoque.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Estoque.GetAll;

public class GetAllItemEstoqueHandler : IRequestHandler<GetAllItemEstoqueQuery, Result<List<ItemEstoqueResponse>>>
{
    private readonly IMapper _mapper;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<GetAllItemEstoqueHandler> _logger;
    private readonly IValidator<GetAllItemEstoqueQuery> _validator;
    private const string UseCaseLogPrefix = "[Get All Item Estoque]";

    public GetAllItemEstoqueHandler(IMapper mapper, IEstoqueRepository estoqueRepository, ILogger<GetAllItemEstoqueHandler> logger, IValidator<GetAllItemEstoqueQuery> validator)
    {
        _mapper = mapper;
        _estoqueRepository = estoqueRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<List<ItemEstoqueResponse>>> Handle(GetAllItemEstoqueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de obtenção de todos os itens de estoque para a empresa. EmpresaId: {EmpresaId}", UseCaseLogPrefix, request.EmpresaId);

        _logger.LogInformation("{UseCaseLogPrefix} Validando request. EmpresaId: {EmpresaId}", UseCaseLogPrefix, request.EmpresaId);
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Validação do request falhou. EmpresaId: {EmpresaId}. Erros: {Errors}", UseCaseLogPrefix, request.EmpresaId, validationResult.Errors);
            var erros = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result.Fail<List<ItemEstoqueResponse>>(new ValidationError(erros));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Validação do request concluída com sucesso. EmpresaId: {EmpresaId}", UseCaseLogPrefix, request.EmpresaId);

        _logger.LogInformation("{UseCaseLogPrefix} Consultando repositório para obter itens de estoque. EmpresaId: {EmpresaId}", UseCaseLogPrefix, request.EmpresaId);
        var itensEstoqueResult = await _estoqueRepository.GetAllItemEstoqueByEmpresaIdAsync(request.EmpresaId);
        if (itensEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Falha ao obter itens de estoque do repositório. EmpresaId: {EmpresaId}. Erros: {Errors}", UseCaseLogPrefix, request.EmpresaId, itensEstoqueResult.Errors);
            return Result.Fail<List<ItemEstoqueResponse>>(new NotFoundError("Nenhum item encontrado no banco de dados"));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Itens de estoque obtidos com sucesso do repositório. EmpresaId: {EmpresaId}, QuantidadeItens: {QuantidadeItens}", UseCaseLogPrefix, request.EmpresaId, itensEstoqueResult.Value.Count);

        _logger.LogInformation("{UseCaseLogPrefix} Mapeando entidades de ItemEstoque para ItemEstoqueResponse. EmpresaId: {EmpresaId}", UseCaseLogPrefix, request.EmpresaId);
        var itensEstoqueResponse = _mapper.Map<List<ItemEstoqueResponse>>(itensEstoqueResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído com sucesso. EmpresaId: {EmpresaId}, QuantidadeItensResponse: {QuantidadeItensResponse}", UseCaseLogPrefix, request.EmpresaId, itensEstoqueResponse.Count);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de obtenção de todos os itens de estoque concluído com sucesso. EmpresaId: {EmpresaId}", UseCaseLogPrefix, request.EmpresaId);
        return Result.Ok(itensEstoqueResponse);
    }
}
