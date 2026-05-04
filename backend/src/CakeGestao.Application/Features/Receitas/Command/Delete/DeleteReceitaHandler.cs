using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Receitas.Command.Delete;

public class DeleteReceitaHandler : IRequestHandler<DeleteReceitaCommand, Result>
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly IValidator<DeleteReceitaCommand> _validator;
    private readonly ILogger<DeleteReceitaHandler> _logger;
    private const string LogPrefix = "[DeleteReceitaHandler]";

    public DeleteReceitaHandler(IReceitaRepository receitaRepository, IValidator<DeleteReceitaCommand> validator, ILogger<DeleteReceitaHandler> logger)
    {
        _receitaRepository = receitaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteReceitaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{LogPrefix} Deletando a receita de id: {ReceitaId} da empresa de id: {EmpresaId}", LogPrefix, request.ReceitaId, request.EmpresaId);
        
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {   
            var errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
            _logger.LogWarning("{LogPrefix} Validação falhou. Erros: {Erros}", LogPrefix, string.Join(", ", errors));
            return Result.Fail(new ValidationError(errors));
        }
        
        var receitaResult = await _receitaRepository.GetReceitaByIdAsync(request.ReceitaId, request.EmpresaId);
        if (receitaResult.IsFailed)
        {
            _logger.LogWarning("{LogPrefix} Receita de id {ReceitaId}não encontrado no banco de dados", LogPrefix, request.ReceitaId);
            return Result.Fail(new NotFoundError("Receita não encontrada"));
        }

        await _receitaRepository.DeleteReceitaAsync(receitaResult.Value);
        
        _logger.LogInformation("{LogPrefix} Receita de id: {ReceitaId} deletado com sucesso",LogPrefix ,request.ReceitaId);
        return Result.Ok();
    }
}