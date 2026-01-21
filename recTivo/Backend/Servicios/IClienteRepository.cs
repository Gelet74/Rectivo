namespace recTivo.Backend.Repos
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<IEnumerable<Cliente>> GetAllAsync();

        Task<Cliente?> GetByDniAsync(string dni);

        Task<Cliente?> GetByUsuarioAsync(string usuario);

        Task<bool> UsuarioExisteAsync(string usuario);

        Task<Cliente?> LoginAsync(string usuario, string password);
    }
}
