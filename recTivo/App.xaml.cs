using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.Backend.Servicios;
using recTivo.Frontend.Dialogos;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.Empleado;
using System.Windows;

namespace recTivo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private RectivoContext _contexto;
        /// Propiedad para almacenar el proveedor de servicios
        private IServiceProvider _serviceProvider;
        /// <summary>
        /// Constructor de la clase App
        /// </summary>
        public App()
        {
            // Configurar el contenedor de inyección de dependencias
            var serviceCollection = new ServiceCollection();
            // Configurar los servicios
            ConfigureServices(serviceCollection);
            // Construir el proveedor de servicios
            _serviceProvider = serviceCollection.BuildServiceProvider();
            
        }
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<RectivoContext>();
            
            services.AddLogging(configure => configure.AddConsole());
            
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

            
            services.AddScoped<IRepository<Articulo>, ArticuloRepository>();
            services.AddScoped<IRepository<Empleado>, EmpleadoRepository>();
            services.AddScoped<IRepository<Cliente>, ClienteRepository>();
            services.AddScoped<IRepository<Orden>, OrdenRepository>();
            services.AddScoped<IRepository<Escandallo>, EscandalloRepository>();
            services.AddScoped<EscandalloRepository>();



            services.AddScoped<ArticuloRepository>();
            services.AddScoped<EmpleadoRepository>();
            services.AddScoped<ClienteRepository>();
            services.AddScoped<OrdenRepository>();
            services.AddScoped<ClienteHasArticuloRepository>();

            
            services.AddSingleton<Inicio>();
            
            services.AddTransient<Login>(); 
            
            
            
            services.AddTransient<DialogoAltaEmpleado>();
            services.AddTransient<DialogoBajaEmpleado>();
            services.AddTransient<DialogoConsultaEmpleado>();
            services.AddTransient<DialogoModificarEmpleado>();

            services.AddTransient<DialogoAltaArticulo>();
            services.AddTransient<DialogoBajaArticulo>();
            services.AddTransient<DialogoModificarArticulo>();
            services.AddTransient<DialogoListarArticulo>();

            services.AddTransient<DialogoEntradaAlmacen>();
            services.AddTransient<DialogoSalidaAlmacen>();

            
        }



        protected override void OnStartup(StartupEventArgs e)
        {
            
            var inicioWindow = _serviceProvider.GetService<Inicio>();
            inicioWindow.Show();
            base.OnStartup(e);
        }
    }
}

