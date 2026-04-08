using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Receitas.Command.Delete;

public class DeleteReceitaValidator : AbstractValidator<DeleteReceitaCommand>
{
    public DeleteReceitaValidator(IEmpresaRepository receitaRepository)
    {
        RuleFor(r => r.EmpresaId)
            .DeveExistirEmpresa(receitaRepository);

        RuleFor(r => r.ReceitaId)
            .NotNull().WithMessage("O Id da receita deve ser informado");
    }
}