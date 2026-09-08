using CakeGestao.Application.Common;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.User.Command.Delete;

public class DeleteUsuarioValidator : AbstractValidator<DeleteUsuarioCommand>
{
    public DeleteUsuarioValidator(IEmpresaRepository empresaRepository)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("O ID do usuário é inválido.");

        RuleFor(x => x.EmpresaId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("O ID da empresa é inválido.")
            .DeveExistirEmpresa(empresaRepository);
    }
}
