using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IUsuarioRepository
{
    public Task<Result> ExistUsuarioByEmailAsync(string email);
    public Task<Result<Usuario>> GetByIdAsync(int id);
    public Task<Result> CreateUserAsync(Usuario usuario);
}
