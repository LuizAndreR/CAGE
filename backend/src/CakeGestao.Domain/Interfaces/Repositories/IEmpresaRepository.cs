using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IEmpresaRepository
{
    public Task<Result<Empresa>> GetEmpresaByIdAsync(int empresaId);
    public Task<Result> EmpresaExistsByNomeAsync(string nome);
    public Task<Result> EmpresaExistsByIdAsync(int empresaId);
    public Task<List<Empresa>> GetAllEmpresasAsync();
    public Task CreateEmpresaAsync(Empresa empresa);
}
