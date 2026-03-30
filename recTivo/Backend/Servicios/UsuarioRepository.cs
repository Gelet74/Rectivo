using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using recTivo.Backend.Modelos;
using recTivo.Backend.Servicios;
using recTivo.Backend.Repos;

namespace recTivo.Backend.Servicios
{
    public class UsuarioRepository : GenericRepository<Empleado>
    {
        public UsuarioRepository(RectivoContext context, ILogger<GenericRepository<Empleado>> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Autentica un usuario verificando la contraseña con BCrypt.
        /// Nunca compara contraseñas en texto plano.
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var usuario = await Query(asNoTracking: true)
                    .FirstOrDefaultAsync(u => u.Username == username)
                    .ConfigureAwait(false);

                // CORRECCIÓN: usar PasswordService.Verify (BCrypt), no comparación directa
                if (usuario == null || string.IsNullOrEmpty(usuario.Password))
                    return false;

                return PasswordService.Verify(password, usuario.Password);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al autenticar usuario {Username}.", username);
                throw;
            }
        }

        /// <summary>
        /// Cambia la contraseña de un usuario verificando la contraseña actual con BCrypt
        /// y guardando la nueva también hasheada.
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("La nueva contraseña no puede estar vacía.", nameof(newPassword));

            try
            {
                var usuario = await GetByIdAsync(userId).ConfigureAwait(false);
                if (usuario == null)
                {
                    _logger?.LogWarning("Cambio de contraseña: usuario con id {Id} no encontrado.", userId);
                    return false;
                }

                // CORRECCIÓN: verificar con BCrypt, no comparando strings
                if (!PasswordService.Verify(currentPassword, usuario.Password!))
                {
                    _logger?.LogWarning("Cambio de contraseña fallido: contraseña actual incorrecta para usuario id {Id}.", userId);
                    return false;
                }

                // CORRECCIÓN: guardar la nueva contraseña hasheada, nunca en texto plano
                usuario.Password = PasswordService.Hash(newPassword);
                _context.Update(usuario);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                _logger?.LogInformation("Contraseña actualizada correctamente para usuario id {Id}.", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cambiar la contraseña del usuario id {Id}.", userId);
                throw;
            }
        }

        public async Task<Empleado?> GetByUsernameAsync(string username)
            => await Query(asNoTracking: true)
                     .FirstOrDefaultAsync(u => u.Username == username)
                     .ConfigureAwait(false);
    }
}