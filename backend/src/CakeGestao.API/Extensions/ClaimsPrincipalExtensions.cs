using System.Security.Claims;
using FluentResults;

namespace CakeGestao.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Result<int> GetUserId(this ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim == null)
            return Result.Fail("Token Invalido");

        if (int.TryParse(idClaim.Value, out var userId))
        {
            return Result.Ok(userId);
        }

        return Result.Fail("Token Invalido");
    }

    public static Result<int> GetEmpresaId(this ClaimsPrincipal principal)
    {
        var empresaIdClaim = principal.FindFirst("EmpresaId");
        if (empresaIdClaim == null)
            return Result.Fail("Token Invalido");
        if (int.TryParse(empresaIdClaim.Value, out var empresaId))
        {
            return Result.Ok(empresaId);
        }
        return Result.Fail("Token Invalido");
    }
}