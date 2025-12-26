using AutoMapper;
using CakeGestao.Application.Features.Empresas.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Empresas.Get;

public class GetEmpresaHandler : IRequestHandler<GetEmpresaQuery, Result<EmpresaResponse>>
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly ILogger<GetEmpresaHandler> _logger;
    private readonly IMapper _mapper;
    private const string UseCaseLogPrefix = "[Get Empresa By Id]";

    public GetEmpresaHandler(IEmpresaRepository empresaRepository, ILogger<GetEmpresaHandler> logger, IMapper mapper)
    {
        _empresaRepository = empresaRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<EmpresaResponse>> Handle(GetEmpresaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{UseCaseLogPrefix} Iniciando processo para a empresa de id: {Id}", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Buscando a empresa de id: {Id} no banco de dados", UseCaseLogPrefix, request.Id);
        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(request.Id);
        if (empresaResult.IsFailed)
        {
            _logger.LogWarning("{UseCaseLogPrefix} Empresa de id: {Id} não encontrada no banco de dados", UseCaseLogPrefix, request.Id);
            return Result.Fail(empresaResult.Errors);
        }
        _logger.LogInformation("{UseCaseLogPrefix} Empresa de id: {Id} encontrada com sucesso", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Iniciando mapeamento da entidade para EmpresaResponse para a empresa de id: {Id}", UseCaseLogPrefix, request.Id);
        var empresaResponse = _mapper.Map<EmpresaResponse>(empresaResult.Value);
        _logger.LogInformation("{UseCaseLogPrefix} Mapeamento para a empresa de id: {Id} concluído com sucesso", UseCaseLogPrefix, request.Id);

        _logger.LogInformation("{UseCaseLogPrefix} Processo para a empresa de id: {Id} finalizado com sucesso", UseCaseLogPrefix, request.Id);
        return Result.Ok(empresaResponse);
    }
}
