using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IEmpresaRepository
{
    public Task<Result<Empresa>> GetEmpresaByIdAsync(int empresaId);
    public Task<Result> EmpresaExistsByNomeAsync(string nome);
    public Task<Result> EmpresaExistsByIdAsync(int empresaId);
    public Task<Result<List<Empresa>>>GetAllEmpresasAsync();
    public Task CreateEmpresaAsync(Empresa empresa);
    public Task UpdateEmpresaAsync(Empresa empresa);
    public Task DeleteEmpresaAsync(Empresa empresa);
}
