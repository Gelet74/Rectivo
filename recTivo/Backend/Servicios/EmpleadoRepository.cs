using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using recTivo.Backend.Modelos;
using System.Threading.Tasks;

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

        // Comparación directa (texto plano)
        if (empleado.Password == password)
            return empleado;

        return null;
    }


}

