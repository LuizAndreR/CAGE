using CakeGestao.Domain.Entities;
using CakeGestao.Domain.Interfaces.Repositories;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CakeGestao.Infrastructure.Data.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly CageContext _context;
    private readonly ILogger<UsuarioRepository> _logger;

    public UsuarioRepository(CageContext context, ILogger<UsuarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Usuario>>> GetAllUsuariosAsync()
    {
        _logger.LogInformation("Iniciando a busca no banco de dados");
        var usuarios = await _context.Usuarios.ToListAsync();
        if (usuarios.Count == 0)
        {
            _logger.LogInformation("Nenhum usuario encontrado");
            return Result.Fail<List<Usuario>>("Nenhum usuario encontrado.");
        }
        _logger.LogInformation("Usuario encontrado com sucesso");
        return Result.Ok(usuarios);
    }
    
    public async Task<Result<Usuario>> GetUsuarioByEmailAsync(string email)
    {
        _logger.LogInformation("Verificando a existencia do usuario no banco de dados: {Email}", email);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null)
        {
            _logger.LogInformation("Usuario não encontrado no banco de dados: {Email}", email);
            return Result.Fail("Usuário não encontrado.");
        }

        _logger.LogInformation("Usuario encontrado no banco de dados: {Email}", email);
        return Result.Ok(usuario);
    }

    public async Task<Result<Usuario>> GetByIdAsync(int id)
    {
        _logger.LogInformation("Buscando usuario por ID no banco de dados: {Id}", id);

        var usuario = await _context.Usuarios.FindAsync(id);
        
        if (usuario == null)
        {
            _logger.LogInformation("Usuario não encontrado no banco de dados: {Id}", id);
            return Result.Fail<Usuario>("Usuário não encontrado.");
        }

        _logger.LogInformation("Usuario encontrado no banco de dados: {Id}", id);
        return Result.Ok(usuario);
    }

    public async Task CreateUserAsync(Usuario usuario)
    {
        _logger.LogInformation("Criando um usuario no banco de dados: {Email}", usuario.Email);

        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUsuarioAsync(Usuario usuario)
    {
        _logger.LogInformation("Atualizando usuario no banco de dados: {Email}", usuario.Email);
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Usuario atualizado com sucesso no banco de dados: {Email}", usuario.Email);
    }

    public async Task DeleteAsync(Usuario usuario)
    {
        _logger.LogInformation("Deletando usuario no banco de dados: {Email}", usuario.Email);

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario deletado com sucesso no banco de dados: {Email}", usuario.Email);
    }
}
