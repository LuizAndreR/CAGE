using AutoMapper;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Application.Features.Receitas.Command.Update;

public class UpdateReceitaHandler: IRequestHandler<UpdateReceitaCommand, Result>
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly ILogger<UpdateReceitaHandler> _logger;
    private readonly IValidator<UpdateReceitaCommand> _validator;
    private readonly IMapper _mapper;

    public UpdateReceitaHandler(IReceitaRepository receitaRepository, ILogger<UpdateReceitaHandler> logger, IValidator<UpdateReceitaCommand> validator, IMapper mapper)
    {
        _receitaRepository = receitaRepository;
        _logger = logger;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result> Handle(UpdateReceitaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando processo de atualização da receita: {Nome}", request.Nome);

        _logger.LogInformation("Validando dados da receita: {Nome}", request.Nome);
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validação falhou para a receita: {Nome}", request.Nome);
            return Result.Fail(new ValidationError(validationResult.Errors.Select(e => e.ErrorMessage).ToList()));
        }
        _logger.LogInformation("Dados validados com sucesso para a receita: {Nome}", request.Nome);

        _logger.LogInformation("Varificando existência da receita: {Nome}", request.Nome);
        var existingReceitaResult = await _receitaRepository.GetReceitaByIdAsync(request.IdReceita);
        if (existingReceitaResult.IsFailed)
        {
            _logger.LogWarning("Receita não encontrada para atualização: {Nome}", request.Nome);
            return Result.Fail(new NotFoundError($"Receita com ID {request.IdReceita} não encontrada."));
        }
        _logger.LogInformation("Receita encontrada para atualização: {Nome}", request.Nome);

        _logger.LogInformation("Mapeando dados para atualização da receita: {Nome}", request.Nome);
        var receita = _mapper.Map(request, existingReceitaResult.Value);
        _logger.LogInformation("Dados mapeados com sucesso para a receita: {Nome}", request.Nome);

        var updateResult = await _receitaRepository.UpdateReceitaAsync(receita);
        _logger.LogInformation("Receita atualizada com sucesso: {Nome}", request.Nome);

        return Result.Ok();
    }
}
