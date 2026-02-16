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

        // ⭐ AÑADIR ESTA PROPIEDAD
        public Empleado? EmpleadoActual { get; set; }

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<RectivoContext>(options =>
            {
                options.UseMySQL("server=localhost;database=RECTIVO;user=root;password=mysql;Allow User Variables=True;Treat Tiny As Boolean=False;Default Command Timeout=60;")
                       .EnableSensitiveDataLogging()
                       .LogTo(message => System.Diagnostics.Debug.WriteLine(message),
                              LogLevel.Information);
            },
            ServiceLifetime.Transient);

            services.AddLogging(configure => configure.AddConsole());
            services.AddTransient<MVArticulo>();
            services.AddTransient<MVEmpleado>();
            services.AddSingleton<MVCliente>();
            services.AddTransient<MVAlmacen>(provider =>
            {
                return new MVAlmacen(
                    provider.GetRequiredService<RectivoContext>(),
                    provider.GetRequiredService<MVArticulo>()
                );
            });

            services.AddTransient<MVEscandallo>(provider =>
            {
                return new MVEscandallo(
                    provider.GetRequiredService<EscandalloRepository>(),
                    provider.GetRequiredService<ArticuloRepository>(),
                    provider.GetRequiredService<OrdenRepository>()
                );
            });

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

            services.AddTransient<Inicio>();
            services.AddTransient<Login>();
            services.AddTransient<MainWindow>();

            services.AddTransient<DialogoEntradaAlmacen>(provider =>
            {
                return new DialogoEntradaAlmacen(
                    provider.GetRequiredService<MVAlmacen>()
                );
            });

            services.AddTransient<DialogoSalidaAlmacen>(provider =>
            {
                return new DialogoSalidaAlmacen(
                    provider.GetRequiredService<MVAlmacen>()
                );
            });

            services.AddTransient<DialogoAltaArticulo>();
            services.AddTransient<DialogoBajaArticulo>();
            services.AddTransient<DialogoModificarArticulo>();

            services.AddTransient<DialogoAltaCliente>();
            services.AddTransient<DialogoConsultaCliente>();
            services.AddTransient<DialogoModificarCliente>();

            services.AddTransient<DialogoAltaEmpleado>();
            services.AddTransient<DialogoConsultaEmpleado>();
            services.AddTransient<DialogoModificarEmpleado>();

            services.AddTransient<DialogoAltaEscandallo>(provider =>
            {
                return new DialogoAltaEscandallo(
                    provider.GetRequiredService<MVEscandallo>()
                );
            });

            services.AddTransient<DialogoModificarEscandallo>();
            services.AddTransient<DialogoListarEscandallo>();

            services.AddTransient<ConfirmacionDialogo>();
            services.AddScoped<RolRepository>();

            services.AddSingleton<UCListadoArticulos>();
            services.AddSingleton<UCListadoClientes>();
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