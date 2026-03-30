using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.Frontend.Dialogos;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.Clientes;
using recTivo.Frontend.Dialogos.Empleado;
using recTivo.Frontend.Dialogos.Escandallo;
using recTivo.Frontend.Dialogos.Ordenes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.Frontend.Dialogos.Ventas;
using recTivo.Frontend.UC;
using recTivo.MVVM;
using System.IO;
using System.Windows;

namespace recTivo
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public Empleado? EmpleadoActual { get; set; }

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // CONFIGURACIÓN — lee appsettings.json desde la carpeta del ejecutable
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // CONTEXTO
            var connectionString = configuration.GetConnectionString("RectivoDb")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'RectivoDb' en appsettings.json.");

            services.AddDbContext<RectivoContext>(options =>
            {
                options.UseMySQL(connectionString)
                       .EnableSensitiveDataLogging()
                       .LogTo(message => System.Diagnostics.Debug.WriteLine(message),
                              LogLevel.Information);
            },
            ServiceLifetime.Transient);

            services.AddLogging(configure => configure.AddConsole());

            // REPOSITORIOS
            services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddTransient<ArticuloRepository>();
            services.AddTransient<EmpleadoRepository>();
            services.AddTransient<ClienteRepository>();
            services.AddTransient<OrdenRepository>();
            services.AddTransient<EscandalloRepository>();
            services.AddTransient<OrdenFaseRepository>();
            services.AddTransient<PedidoRepository>();
            services.AddTransient<RolRepository>();

            // USER CONTROLS
            services.AddTransient<UCCerrarFase>();
            services.AddTransient<UCListadoOrdenes>();
            services.AddTransient<UCListadoArticulos>();
            services.AddTransient<UCListadoClientes>();
            services.AddTransient<UCDashboard>();

            // VIEWMODELS
            services.AddTransient<MVArticulo>();
            services.AddTransient<MVEmpleado>();
            services.AddTransient<MVCliente>();
            services.AddTransient<MVAlmacen>(provider => new MVAlmacen(
                provider.GetRequiredService<RectivoContext>(),
                provider.GetRequiredService<MVArticulo>()
            ));
            services.AddTransient<MVEscandallo>(provider => new MVEscandallo(
                provider.GetRequiredService<EscandalloRepository>(),
                provider.GetRequiredService<ArticuloRepository>(),
                provider.GetRequiredService<OrdenRepository>()
            ));
            services.AddTransient<MVOrden>(provider => new MVOrden(
                provider.GetRequiredService<EscandalloRepository>(),
                provider.GetRequiredService<ArticuloRepository>(),
                provider.GetRequiredService<OrdenRepository>(),
                provider.GetRequiredService<EmpleadoRepository>(),
                provider.GetRequiredService<OrdenFaseRepository>()
            ));

            // VENTANAS
            services.AddTransient<Inicio>();
            services.AddTransient<Login>();
            services.AddTransient<MainWindow>();

            // DIÁLOGOS DE ALMACÉN
            services.AddTransient<DialogoEntradaAlmacen>(provider => new DialogoEntradaAlmacen(
                provider.GetRequiredService<MVAlmacen>()
            ));
            services.AddTransient<DialogoSalidaAlmacen>(provider => new DialogoSalidaAlmacen(
                provider.GetRequiredService<MVAlmacen>()
            ));

            // DIÁLOGOS DE ARTÍCULOS
            services.AddTransient<DialogoAltaArticulo>();
            services.AddTransient<DialogoBajaArticulo>();
            services.AddTransient<DialogoModificarArticulo>();

            // DIÁLOGOS DE CLIENTES
            services.AddTransient<DialogoAltaCliente>();
            services.AddTransient<DialogoModificarCliente>();

            // DIÁLOGOS DE EMPLEADOS
            services.AddTransient<DialogoAltaEmpleado>();
            services.AddTransient<DialogoConsultaEmpleado>();
            services.AddTransient<DialogoModificarEmpleado>();

            // DIÁLOGOS DE ESCANDALLO
            services.AddTransient<DialogoAltaEscandallo>(provider => new DialogoAltaEscandallo(
                provider.GetRequiredService<MVEscandallo>()
            ));
            services.AddTransient<DialogoModificarEscandallo>(provider => new DialogoModificarEscandallo(
                provider.GetRequiredService<MVEscandallo>()
            ));
            services.AddTransient<DialogoListarEscandallo>();

            // DIÁLOGOS DE ÓRDENES
            services.AddTransient<DialogoProcesarOrden>(provider => new DialogoProcesarOrden(
                provider.GetRequiredService<MVOrden>()
            ));

            // DIÁLOGOS DE PEDIDOS
            services.AddTransient<MVPedido>(provider => new MVPedido(
                provider.GetRequiredService<PedidoRepository>(),
                provider.GetRequiredService<ArticuloRepository>(),
                provider.GetRequiredService<EscandalloRepository>(),
                provider.GetRequiredService<ClienteRepository>(),
                provider.GetRequiredService<OrdenRepository>(),
                provider.GetRequiredService<OrdenFaseRepository>()
            ));
            services.AddTransient<DialogoPedidos>(provider => new DialogoPedidos(
                provider.GetRequiredService<MVPedido>()
            ));

            services.AddTransient<ConfirmacionDialogo>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var login = _serviceProvider.GetRequiredService<Inicio>();
            login.Show();
        }
    }
}