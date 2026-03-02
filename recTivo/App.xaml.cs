using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.Backend.Servicios;
using recTivo.Frontend.Dialogos;
using recTivo.Frontend.Dialogos.Articulos;
using recTivo.Frontend.Dialogos.Clientes;
using recTivo.Frontend.Dialogos.Empleado;
using recTivo.Frontend.Dialogos.Escandallo;
using recTivo.Frontend.Dialogos.Ordenes;
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.Frontend.UC;
using recTivo.MVVM;
using System;
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
            // CONTEXTO: Transient para evitar contextos compartidos en WPF
            services.AddDbContext<RectivoContext>(options =>
            {
                options.UseMySQL("server=localhost;database=RECTIVO;user=root;password=mysql;Allow User Variables=True;Treat Tiny As Boolean=False;Default Command Timeout=60;")
                       .EnableSensitiveDataLogging()
                       .LogTo(message => System.Diagnostics.Debug.WriteLine(message),
                              LogLevel.Information);
            },
            ServiceLifetime.Transient);

            services.AddLogging(configure => configure.AddConsole());

            // REPOSITORIOS: todos Transient, consistente con el contexto
            services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddTransient<ArticuloRepository>();
            services.AddTransient<EmpleadoRepository>();
            services.AddTransient<ClienteRepository>();
            services.AddTransient<OrdenRepository>();
            services.AddTransient<EscandalloRepository>();
            services.AddTransient<OrdenFaseRepository>();
            services.AddTransient<MVOrden>();
            services.AddTransient<UCCerrarFase>();
            services.AddTransient<UCListadoOrdenes>();

            services.AddTransient<UCListadoOrdenes>();
            services.AddTransient<RolRepository>();

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
                provider.GetRequiredService<OrdenFaseRepository>(),
                provider.GetRequiredService<RectivoContext>()
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
            services.AddTransient<DialogoConsultaCliente>();
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

            services.AddTransient<ConfirmacionDialogo>();

            // USER CONTROLS
            services.AddTransient<UCListadoArticulos>();
            services.AddTransient<UCListadoClientes>();
            services.AddTransient<UCDashboard>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var login = _serviceProvider.GetRequiredService<Login>();
            login.Show();
        }
    }
}