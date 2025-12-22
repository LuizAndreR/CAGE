using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.UseCases.Estoque.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Estoque.UseCase;

public class GetAllItemEstoqueUseCase : IGetAllItemEstoqueUseCase
{
    private readonly IMapper _mapper;
    private readonly IEstoqueRepository _estoqueRepository;
    private readonly ILogger<GetAllItemEstoqueUseCase> _logger;
    private const string UseCaseLogPrefix = "[Get All Item Estoque]";

    public GetAllItemEstoqueUseCase(IMapper mapper, IEstoqueRepository estoqueRepository, ILogger<GetAllItemEstoqueUseCase> logger)
    {
        _mapper = mapper;
        _estoqueRepository = estoqueRepository;
        _logger = logger;
    }

    public async Task<Result<List<ItemEstoqueResponse>>> ExecuteAsync(int empresaId)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo de obtenção de todos os itens de estoque para a empresa. EmpresaId: {EmpresaId}", UseCaseLogPrefix, empresaId);

        _logger.LogInformation("{UseCaseLogPrefix} Consultando repositório para obter itens de estoque. EmpresaId: {EmpresaId}", UseCaseLogPrefix, empresaId);
        var itensEstoqueResult = await _estoqueRepository.GetAllItemEstoqueByEmpresaIdAsync(empresaId);
        if (itensEstoqueResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Falha ao obter itens de estoque do repositório. EmpresaId: {EmpresaId}. Erros: {Errors}", UseCaseLogPrefix, empresaId, itensEstoqueResult.Errors);
            return Result.Fail<List<ItemEstoqueResponse>>(new NotFoundError("Ñenhum item encontrado no banco de dados"));
        }
        _logger.LogInformation("{UseCaseLogPrefix} Itens de estoque obtidos com sucesso do repositório. EmpresaId: {EmpresaId}, QuantidadeItens: {QuantidadeItens}", UseCaseLogPrefix, empresaId, itensEstoqueResult.Value.Count);

        _logger.LogInformation("{UseCaseLogPrefix} Mapeando entidades de ItemEstoque para ItemEstoqueResponse. EmpresaId: {EmpresaId}", UseCaseLogPrefix, empresaId);
        var itensEstoqueResponse = _mapper.Map<List<ItemEstoqueResponse>>(itensEstoqueResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento concluído com sucesso. EmpresaId: {EmpresaId}, QuantidadeItensResponse: {QuantidadeItensResponse}", UseCaseLogPrefix, empresaId, itensEstoqueResponse.Count);

        _logger.LogInformation("{UseCaseLogPrefix} Processo de obtenção de todos os itens de estoque concluído com sucesso. EmpresaId: {EmpresaId}", UseCaseLogPrefix, empresaId);
        return Result.Ok(itensEstoqueResponse);
    }
}
