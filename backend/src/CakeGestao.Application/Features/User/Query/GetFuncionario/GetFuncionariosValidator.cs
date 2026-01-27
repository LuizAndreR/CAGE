using CakeGestao.Application.Common;
using CakeGestao.Infrastructure.Data.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.User.Query.GetFuncionario;

public class GetFuncionariosValidator : AbstractValidator<GetFuncionariosQuery>
{
    public GetFuncionariosValidator(EmpresaRepository empresaR)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("O ID da empresa deve ser maior que zero.")
            .DeveExistirEmpresa(empresaR);
    }
}
