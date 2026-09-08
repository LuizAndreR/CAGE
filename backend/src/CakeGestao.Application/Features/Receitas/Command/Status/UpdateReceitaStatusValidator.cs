using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Receitas.Command.Status;

public class UpdateReceitaStatusValidator : AbstractValidator<UpdateReceitaStatusCommand>
{
    public UpdateReceitaStatusValidator(IEmpresaRepository repository)
    {
        RuleFor(r => r.EmpresaId)
            .DeveExistirEmpresa(repository);
    }
}