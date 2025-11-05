using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IUsuarioRepository
{
    public Task<Result<List<Usuario>>> GetAllUsuariosAsync();
    public Task<Result<Usuario>> GetUsuarioByEmailAsync(string email);
    public Task<Result<Usuario>> GetByIdAsync(int id);
    public Task CreateUserAsync(Usuario usuario);
    public Task UpdateUsuarioAsync(Usuario usuario);
    public Task DeleteAsync(Usuario usuario);
}
