using AutoMapper;
using CakeGestao.Application.Dtos.Requests.Empresa;
using CakeGestao.Application.UseCases.Empresas.Interface;
using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.UseCases.Empresas.UseCase;

public class UpdateEmpresaUseCase : IUpdateEmpresaUseCase
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IValidator<UpdateEmpresaRequest> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateEmpresaUseCase> _logger;

    public UpdateEmpresaUseCase(IEmpresaRepository empresaRepository, IValidator<UpdateEmpresaRequest> validator, IMapper mapper, ILogger<UpdateEmpresaUseCase> logger)
    {
        _empresaRepository = empresaRepository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(UpdateEmpresaRequest request, int id)
    {
        _logger.LogInformation("Iniciando processo de update da empresa de id: {Id}", id);

        _logger.LogInformation("Iniciando processo de verificação dos dados fonecidos do update da empresa de id: {Id}", id);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogInformation("Validação falhou para o update da empresa de Id: {Id}", id);
            return Result.Fail(new ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
        }
        _logger.LogInformation("Validação realizado com susseso dos dados fonecido do updade da empresa de id: {Id}", id);

        _logger.LogInformation("Iniciando o processo de verificação de existencia da empresa de id: {Id}", id);
        var empresaResult = await _empresaRepository.GetEmpresaByIdAsync(id);
        if (empresaResult.IsFailed)
        {
            _logger.LogInformation("Não encontra no banco de dados a empresa de id: {Id}", id);
            return Result.Fail(new NotFoundError("Empresa não encontrado no banco de dados"));
        }
        _logger.LogInformation("Verificação realizado com sussesso, empresa de id: {Id} encontrado", id);

        _logger.LogInformation("Iniciando processo de verificação de altereações no update da empresa de id: {Id}", id);
        var empresa = empresaResult.Value;
        var status = Enum.Parse<StatusEmpresaEnum>(request.Status);
        if (empresa.Nome == request.Nome && empresa.Endereco == request.Endereco && empresa.Status == status)
        {
            _logger.LogInformation("Nenhuma alteração encontrado no update da empresa de id: {Id}", id);
            return Result.Ok();
        }
        _logger.LogInformation("Verificação de alteração no update da empresa de id: {Id} realizada com susseso", id);

        _logger.LogInformation("Iniciando o processo de mapeamento da entidade resquest para update da empresa de id: {Id}", id);
        empresa.Nome = request.Nome;
        empresa.Endereco = request.Endereco;
        empresa.Status = status;
        _logger.LogInformation("Mapeamento realizado com susseso da entidade resquest para update da empresa de id: {Id}", id);

        _logger.LogInformation("Iniciando o processo de update da empresa de id {Id} no banco de dados", id);
        await _empresaRepository.UpdateEmpresaAsync(empresa);

        _logger.LogInformation("Processo de update relizado com susseso");
        return Result.Ok();
    }
}
