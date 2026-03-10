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
    private const string LogPrefix = "[Usuario Repository]";

    public UsuarioRepository(CageContext context, ILogger<UsuarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Usuario>>> GetAllUsuariosAsync(int? empresaId)
    {
        _logger.LogInformation("{LogPrefix} Buscando usuários. Filtro EmpresaId: {EmpresaId}", LogPrefix, empresaId);

        var query = _context.Usuarios.AsNoTracking();
        if (empresaId.HasValue)
        {
            query = query.Where(u => u.EmpresaId == empresaId.Value);
        }
        var usuarios = await query.ToListAsync();

        if (usuarios.Count == 0)
        {
            _logger.LogInformation("{LogPrefix} Nenhum usuário encontrado para o filtro.", LogPrefix);
            return Result.Fail("Nenhum usuario encontrado.");
        }
        _logger.LogInformation("{LogPrefix} {Count} usuários encontrados.", LogPrefix, usuarios.Count);
        return Result.Ok(usuarios);
    }
    
    public async Task<Result<Usuario>> GetUsuarioByEmailAsync(string email)
    {
        _logger.LogDebug("{LogPrefix} Verificando a existencia usuário por email: {Email}", LogPrefix, email);

        var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null)
        {
            return Result.Fail("Usuário não encontrado.");
        }

        return Result.Ok(usuario);
    }

    public async Task<Result<Usuario>> GetByIdAsync(int id, int? empresaId)
    {
        _logger.LogInformation("{LogPrefix} Buscando usuario por ID: {Id}", LogPrefix, id);

        var query = _context.Usuarios.AsQueryable();
        if (empresaId.HasValue)
        {
            query = query.Where(u => u.EmpresaId == empresaId.Value);
        }

        var usuario = await query.FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
        {
            _logger.LogWarning("{LogPrefix} Usuário ID {Id} não encontrado (ou não pertence à empresa {EmpresaId}).", LogPrefix, id, empresaId);
            return Result.Fail("Usuário não encontrado.");
        }

        return Result.Ok(usuario);
    }

    public async Task CreateUserAsync(Usuario usuario)
    {
        _logger.LogInformation("{LogPrefix} Inserindo novo usuário: {Email}", LogPrefix, usuario.Email);

        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUsuarioAsync(Usuario usuario)
    {
        _logger.LogInformation("{LogPrefix} Atualizando usuário ID: {Id}", LogPrefix, usuario.Id);

        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Usuario usuario)
    {
        _logger.LogInformation("{LogPrefix} Removendo usuário ID: {Id}", LogPrefix, usuario.Id);

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
    }
}
