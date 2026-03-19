using CakeGestao.Application.Common;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentValidation;

namespace CakeGestao.Application.Features.Financeiro.Query.GetAll;

public class GetAllTransacaoValidator : AbstractValidator<GetAllTransacaoQuery>
{
    public GetAllTransacaoValidator(IEmpresaRepository empresaRepo)
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0).WithMessage("ID da empresa inválido.")
            .DeveExistirEmpresa(empresaRepo);

        RuleFor(x => x.Tipo)
            .IsEnumName(typeof(TipoTransacaoEnum), caseSensitive: false).WithMessage($"Tipo de transação inválido. Valores aceitos: {string.Join(", ", Enum.GetNames(typeof(TipoTransacaoEnum)))}");

        RuleFor(x => x.Categoria)
            .IsEnumName(typeof(CategoriasEnum), caseSensitive: false).WithMessage($"Categoria inválida. Valores aceitos: {string.Join(", ", Enum.GetNames(typeof(CategoriasEnum)))}");
    }
}