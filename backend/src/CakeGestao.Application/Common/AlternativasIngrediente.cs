using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Enum;
using CakeGestao.Domain.Exceptions;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;

namespace CakeGestao.Application.Common;

public static class AlternativasIngrediente
{
    public static bool UnidadesCompativeis(UnidadeMedidaEnum principal, UnidadeMedidaEnum alternativa)
    {
        if (!Enum.IsDefined(principal) || !Enum.IsDefined(alternativa)) return false;
        if (principal == UnidadeMedidaEnum.UN || alternativa == UnidadeMedidaEnum.UN)
            return principal == alternativa;
        if (principal is UnidadeMedidaEnum.CUP or UnidadeMedidaEnum.TSP or UnidadeMedidaEnum.TBS ||
            alternativa is UnidadeMedidaEnum.CUP or UnidadeMedidaEnum.TSP or UnidadeMedidaEnum.TBS)
            return true;
        return (principal is UnidadeMedidaEnum.G or UnidadeMedidaEnum.KG) ==
            (alternativa is UnidadeMedidaEnum.G or UnidadeMedidaEnum.KG);
    }

    public static async Task<Result> ConfigurarAsync(Ingrediente ingrediente, List<int>? ids,
        int empresaId, IEstoqueRepository estoqueRepository)
    {
        if (ids == null || ids.Any(id => id <= 0 || id == ingrediente.ItemId) || ids.Distinct().Count() != ids.Count)
            return Result.Fail(new ValidationError("As alternativas devem ser itens distintos do principal, sem repetições."));

        for (var ordem = 0; ordem < ids.Count; ordem++)
        {
            var result = await estoqueRepository.GetItemEstoqueByIdAsync(ids[ordem], empresaId);
            if (result.IsFailed)
                return Result.Fail(new ValidationError($"Alternativa {ids[ordem]} não encontrada nesta empresa."));
            var item = result.Value;
            if (!UnidadesCompativeis(ingrediente.UnidadeMedida, item.UnidadeMedida))
                return Result.Fail(new ValidationError($"Alternativa {item.Nome} — {item.Marca}: use massa com massa, volume com volume ou medidas culinárias. UN aceita apenas UN."));
            var conversao = ConversorUnidade.Converter(1, ingrediente.UnidadeMedida, item.UnidadeMedida,
                item.UnidadeReferenciaVolume, item.PesoReferenciaEmGramas);
            if (conversao.IsFailed)
                return Result.Fail(new ValidationError($"Alternativa {item.Nome} — {item.Marca}: {conversao.Errors[0].Message}"));
            ingrediente.Alternativas.Add(new(item.Id, ordem));
        }
        return Result.Ok();
    }
}
