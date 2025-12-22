using CakeGestao.Application.Common;
using CakeGestao.Application.Dtos.Requests.Estoque;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Validators.Estoque;

public class AddQuantidadeEstoqueValidator : AbstractValidator<AddQuantidadeEstoqueRequest>
{
    public AddQuantidadeEstoqueValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);

        RuleFor(x => x.EstoqueId)
            .GreaterThan(0).WithMessage("ID do estoque inválido.");

        RuleFor(x => x.QuantidadeAdicionar)
            .GreaterThan(0).WithMessage("A quantidade a ser adicionada deve ser maior que zero.");

        RuleFor(x => x.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor não pode ser negativo.");
    }
}
