using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Receitas.Command.Status;

public class UpdateReceitaStatusHandler : IRequestHandler<UpdateReceitaStatusCommand, Result>
{
    private readonly IValidator<UpdateReceitaStatusCommand> _validator;
    private readonly ILogger<UpdateReceitaStatusHandler> _logger;
    private readonly IReceitaRepository _repository;
    private const string LogPrefix = "[UpdateReceitaStatusHandler]";

    public UpdateReceitaStatusHandler(IValidator<UpdateReceitaStatusCommand> validator, ILogger<UpdateReceitaStatusHandler> logger, IReceitaRepository repository)
    {
        _validator = validator;
        _logger = logger;
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateReceitaStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Alterando status da receita {Id} para {Status}", LogPrefix, request.EmpresaId, request.Status);
        
        var validatorResult =  await _validator.ValidateAsync(request, cancellationToken);
        if (!validatorResult.IsValid)
        {
            var errors = validatorResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou. Erros: {Erros}", LogPrefix, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }
        
        var receitaResult = await _repository.GetReceitaByIdAsync(request.EmpresaId, request.ReceitaId);
        if (receitaResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Receita de id {ReceitaId}não encontrado no banco de dados", LogPrefix, request.ReceitaId);
            return Result.Fail(new NotFoundError("Receita não encontrada"));
        }
        Receita receita = receitaResult.Value;
        
        receita.AlteraStatus(request.Status);
        
        await _repository.UpdateReceitaAsync(receita);

        _logger.LogInformation("{LogPrefix} O status da receita de id: {ReceitaId} foi alterada para: {Statis}", LogPrefix, request.ReceitaId, request.Status);
        return Result.Ok();
    }
}