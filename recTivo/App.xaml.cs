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
using recTivo.Frontend.Dialogos.VentanasInicio;
using recTivo.MVVM;
using System;
using System.Windows;

namespace recTivo
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<RectivoContext>(options =>
            options.UseMySQL("server=localhost;database=RECTIVO;user=root;password=mysql;Allow User Variables=True;Treat Tiny As Boolean=False;Default Command Timeout=60;"),
            ServiceLifetime.Transient);

            services.AddLogging(configure => configure.AddConsole());
            services.AddTransient<MVArticulo>();


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

            services.AddTransient<DialogoEntradaAlmacen>();
            services.AddTransient<DialogoSalidaAlmacen>();

            services.AddTransient<DialogoAltaArticulo>();
            services.AddTransient<DialogoBajaArticulo>();
            services.AddTransient<DialogoModificarArticulo>();
            services.AddTransient<DialogoListarArticulo>();

            services.AddTransient<DialogoAltaClientes>();
            services.AddTransient<DialogoBajaCliente>();
            services.AddTransient<DialogoConsultaCliente>();
            services.AddTransient<DialogoModificarCliente>();

            services.AddTransient<DialogoAltaEmpleado>();
            services.AddTransient<DialogoBajaEmpleado>();
            services.AddTransient<DialogoConsultaEmpleado>();
            services.AddTransient<DialogoModificarEmpleado>();

            services.AddTransient<ConfirmacionDialogo>();

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var inicioWindow = _serviceProvider.GetService<Inicio>();
            inicioWindow.Show();

            base.OnStartup(e);
        }

    }
}
