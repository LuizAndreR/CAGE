using CakeGestao.Domain.Enum;
using FluentResults;
using UnitsNet;
using UnitsNet.Units;

namespace CakeGestao.Application.Common;

public static class ConversorUnidade
{
    public static Result<decimal> Converter(
        decimal quantidade,
        UnidadeMedidaEnum origem,
        UnidadeMedidaEnum destino,
        UnidadeMedidaEnum? unidadeReferenciaVolume = null,
        decimal? pesoReferenciaEmGramas = null)
    {
        // 1. Passa direto: 1 para 1 (Ex: UN para UN, KG para KG)
        if (origem == destino) return Result.Ok(quantidade);

        // 2. Bloqueio de negócio: Impede misturar Unidade discreta com Massa/Volume
        if (origem == UnidadeMedidaEnum.UN || destino == UnidadeMedidaEnum.UN)
        {
            return Result.Fail("Não é possível converter 'Unidades' (UN) para medidas de massa ou volume. Por favor, verifique se a unidade informada na receita corresponde à unidade cadastrada no estoque.");
        }

        // 3. Lógica original de conversão contínua
        bool origemEMassa = IsMassa(origem);
        bool destinoEMassa = IsMassa(destino);
        double qtd = (double)quantidade;

        if (origemEMassa == destinoEMassa)
        {
            return origemEMassa
                ? Result.Ok((decimal)new Mass(qtd, ObterUnidadeMassa(origem)).As(ObterUnidadeMassa(destino)))
                : Result.Ok((decimal)new Volume(qtd, ObterUnidadeVolume(origem)).As(ObterUnidadeVolume(destino)));
        }
    
        if (unidadeReferenciaVolume is null || pesoReferenciaEmGramas is null || pesoReferenciaEmGramas <= 0)
        {
            return Result.Fail("Para usar medidas de volume (como Xícara, Colher ou ml) neste ingrediente, você precisa editar o cadastro dele no Estoque e informar a Referência de Peso (Ex: 1 CUP = 120g).");
        }
        
        double densidadeGramaPorMl = (double)pesoReferenciaEmGramas.Value / new Volume(1, ObterUnidadeVolume(unidadeReferenciaVolume.Value)).Milliliters;

        if (origemEMassa)
        {
            double emMl = new Mass(qtd, ObterUnidadeMassa(origem)).Grams / densidadeGramaPorMl;
            return Result.Ok((decimal)Volume.FromMilliliters(emMl).As(ObterUnidadeVolume(destino)));
        }

        double emGramas = new Volume(qtd, ObterUnidadeVolume(origem)).Milliliters * densidadeGramaPorMl;
        return Result.Ok((decimal)Mass.FromGrams(emGramas).As(ObterUnidadeMassa(destino)));
    }

    private static bool IsMassa(UnidadeMedidaEnum unidade) =>
        unidade is UnidadeMedidaEnum.G or UnidadeMedidaEnum.KG;

    private static MassUnit ObterUnidadeMassa(UnidadeMedidaEnum unidade) => unidade switch
    {
        UnidadeMedidaEnum.G => MassUnit.Gram,
        UnidadeMedidaEnum.KG => MassUnit.Kilogram,
        _ => throw new ArgumentException($"Unidade de massa inválida: {unidade}")
    };

    private static VolumeUnit ObterUnidadeVolume(UnidadeMedidaEnum unidade) => unidade switch
    {
        UnidadeMedidaEnum.ML => VolumeUnit.Milliliter,
        UnidadeMedidaEnum.L => VolumeUnit.Liter,
        UnidadeMedidaEnum.TSP => VolumeUnit.MetricTeaspoon,
        UnidadeMedidaEnum.TBS => VolumeUnit.UkTablespoon,
        UnidadeMedidaEnum.CUP => VolumeUnit.MetricCup,
        _ => throw new ArgumentException($"Unidade de volume inválida: {unidade}")
    };
}