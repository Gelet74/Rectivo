using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using recTivo.Backend.Modelos;
using recTivo.Backend.Servicios;

namespace recTivo.Backend.Repos;

public class EmpleadoRepository : GenericRepository<Empleado>
{
    private readonly ILogger<GenericRepository<Empleado>> _logger;

    public EmpleadoRepository(RectivoContext context, ILogger<GenericRepository<Empleado>> logger) 
        : base(context, logger)
    {
    }

    public async Task<Empleado?> GetByUsernameAsync(string username)
        => await _dbSet.FirstOrDefaultAsync(e => e.Username == username);

    public async Task<Empleado?> ValidarCredencialesAsync(string username, string password)
    {
        _logger?.LogInformation("Validando credenciales para {Username}", username);

        var empleado = await _dbSet.FirstOrDefaultAsync(e => e.Username == username);

        if (empleado == null)
            return null;

        if (PasswordService.Verify(password, empleado.Password))
            return empleado;

        return null;
    }

    public async Task<IEnumerable<Empleado>> GetAllAsync()
    {
        return await _context.Empleados
                             .Include(e => e.Rol)
                             .ToListAsync();
    }
    public async Task DeleteAsync(int id)
    {
        var empleado = await _dbSet.FindAsync(id);

        if (empleado != null)
        {
            _dbSet.Remove(empleado);
            await _context.SaveChangesAsync();
        }
    }
}