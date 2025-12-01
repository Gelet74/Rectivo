using System.Threading;
using System.Threading.Tasks;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace recTivo.Backend.Servicios
{
    public interface IUsuarioRepository : IRepository<Empleado>
    {
        /// <summary>
        /// Intenta autenticar un usuario por nombre y contraseña.
        /// Devuelve la entidad <see cref="Usuario"/> si las credenciales son correctas, o null en caso contrario.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="password">Contraseña (texto plano o ya hasheada según la política de la app).</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task<bool> LoginAsync(string username, string password);

        /// <summary>
        /// Cambia la contraseña de un usuario verificando la contraseña actual.
        /// Devuelve true si la contraseña se actualizó correctamente, false si no se encontró el usuario o la contraseña actual no coincide.
        /// </summary>
        /// <param name="userId">Id del usuario.</param>
        /// <param name="currentPassword">Contraseña actual (texto plano o hasheada según la política).</param>
        /// <param name="newPassword">Nueva contraseña (texto plano o hasheada según la política).</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario (sin tracking por defecto).
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task<Empleado?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Comprueba si existe un usuario con el nombre proporcionado.
        /// </summary>
        /// <param name="username">Nombre de usuario a comprobar.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
    }
}
