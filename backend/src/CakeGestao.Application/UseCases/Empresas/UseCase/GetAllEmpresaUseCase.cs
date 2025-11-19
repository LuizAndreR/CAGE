using AutoMapper;
using CakeGestao.Application.Dtos.Responses;
using CakeGestao.Application.UseCases.Empresas.Interface;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Empresas.UseCase;

public class GetAllEmpresaUseCase : IGetAllEmpresaUseCase
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ILogger<GetAllEmpresaUseCase> _logger;
    private readonly IMapper _mapper;

    public GetAllEmpresaUseCase(IEmpresaRepository empresaRepository, ILogger<GetAllEmpresaUseCase> logger, IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<List<EmpresaResponse>>> ExecuteAsync()
    {
        _logger.LogInformation("Iniciando o processo de get de tadas as empresa registado no banco de dados");

        _logger.LogInformation("Iniciando o processo de busca de todas as empresa cadastrada no banco de dados");
        var listEmpresaResult = await _empresaRepository.GetAllEmpresasAsync();
        if (listEmpresaResult.IsFailed)
        {
            _logger.LogInformation("Não foi encontrado empresa cadastrada no banco de dados");
            return Result.Fail(new NotFoundError("Não foi encontrado empresa cadastrada no banco de dados "));
        }
        _logger.LogInformation("Busca realizada com sucesso, foi encontrado {Empresa} empresas cadastradas", listEmpresaResult.Value.Count);
        
        _logger.LogInformation("Inicinando o processo de mapeamento das entidades da lista de empreasa");
        var listEmpresa = _mapper.Map<List<EmpresaResponse>>(listEmpresaResult.Value);
        _logger.LogInformation("Mapeamento realizado com susseso da lista de empreasa");
        
        _logger.LogInformation("Processo de GetAll de empresa realizado com sucesso");
        return Result.Ok(listEmpresa);
    }
}