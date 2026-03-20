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
        
        RuleFor(x => x.Mes)
            .InclusiveBetween(1, 12).WithMessage("O mês deve estar entre 1 e 12.")
            .Must((query, mes) => 
            {
                var anoAtual = DateTime.UtcNow.Year;
                var mesAtual = DateTime.UtcNow.Month;

                if (query.Ano == anoAtual)
                {
                    return mes <= mesAtual;
                }
        
                if (query.Ano > anoAtual)
                {
                    return false;
                }

                return true;
            })
            .WithMessage("Não é possível buscar por um mês e ano que ainda não começou.")
            .When(x => x.Mes.HasValue);

        RuleFor(x => x.Mes)
            .NotNull().WithMessage("Você deve informar o Mês quando buscar por Ano.")
            .When(x => x.Ano.HasValue);

        RuleFor(x => x.Ano)
            .NotNull().WithMessage("Você deve informar o Ano quando buscar por Mês.")
            .When(x => x.Mes.HasValue);
    }
}