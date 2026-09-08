using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.User.Query.GetFuncionario;

public class GetFuncionariosValidator : AbstractValidator<GetFuncionariosQuery>
{
    public GetFuncionariosValidator(IEmpresaRepository empresaR)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("O ID da empresa deve ser maior que zero.")
            .DeveExistirEmpresa(empresaR);
    }
}
