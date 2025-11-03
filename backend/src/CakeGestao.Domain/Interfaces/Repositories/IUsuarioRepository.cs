using CakeGestao.Domain.Entities;
using FluentResults;

namespace CakeGestao.Domain.Interfaces.Repositories;

public interface IUsuarioRepository
{
    public Task<Result<Usuario>> GetUsuarioByEmailAsync(string email);
    public Task<Result<Usuario>> GetByIdAsync(int id);
    public Task<Result> CreateUserAsync(Usuario usuario);
    public Task<Result> UpdateUsuarioAsync(Usuario usuario);
}
