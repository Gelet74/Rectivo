using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;

namespace recTivo.Backend.Modelos
{
    public partial class RectivoContext : DbContext
    {
        public RectivoContext(DbContextOptions<RectivoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Articulo> Articulos { get; set; } = null!;
        public virtual DbSet<Cliente> Clientes { get; set; } = null!;
        public virtual DbSet<Empleado> Empleados { get; set; } = null!;
        public virtual DbSet<Orden> Orden { get; set; } = null!;
        public virtual DbSet<OrdenFase> OrdenFases { get; set; } = null!;
        public virtual DbSet<Permiso> Permisos { get; set; } = null!;
        public virtual DbSet<Rol> Rols { get; set; } = null!;
        public virtual DbSet<Ubicacion> Ubicacion { get; set; } = null!;
        public virtual DbSet<ClienteHasArticulo> ClienteHasArticulos { get; set; } = null!;
        public virtual DbSet<Escandallo> Escandallos { get; set; } = null!;
        public virtual DbSet<ComponenteEscandallo> ComponenteEscandallos { get; set; } = null!;
        public virtual DbSet<Pedido> Pedidos { get; set; } = null!;
        public virtual DbSet<PedidoLinea> PedidoLineas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("cliente");
                entity.HasKey(c => c.IdCliente).HasName("PRIMARY");
                entity.Property(c => c.IdCliente).HasColumnName("IDCLIENTE");
                entity.Property(c => c.Nombre).HasColumnName("NOMBRE").HasMaxLength(50);
                entity.Property(c => c.Apellido1).HasColumnName("APELLIDO1").HasMaxLength(50);
                entity.Property(c => c.Apellido2).HasColumnName("APELLIDO2").HasMaxLength(50);
                entity.Property(c => c.NumFactura).HasColumnName("NUM_FACTURA");
                entity.Property(c => c.NumPedido).HasColumnName("NUM_PEDIDO");
                entity.Property(c => c.Dni).HasColumnName("DNI").HasMaxLength(20);
                entity.Property(c => c.Telefono).HasColumnName("TELEFONO").HasMaxLength(20);
                entity.Property(c => c.Usuario).HasColumnName("username").HasMaxLength(50);
                entity.Property(c => c.Password).HasColumnName("password").HasMaxLength(255);
            });

            modelBuilder.Entity<Articulo>(entity =>
            {
                entity.ToTable("articulo");
                entity.HasKey(a => a.IdArticulo).HasName("PRIMARY");
                entity.Property(a => a.IdArticulo).HasColumnName("id_articulo");
                entity.Property(a => a.Codigo).HasColumnName("codigo").HasMaxLength(10).IsRequired();
                entity.Property(a => a.descrip).HasColumnName("descripcion").HasMaxLength(50).IsRequired();
                entity.Property(a => a.descrip2).HasColumnName("descripcion2").HasMaxLength(50); 
                entity.Property(a => a.Stock).HasColumnName("stock").HasColumnType("decimal(10,2)").HasDefaultValue(0m);
                entity.Property(a => a.Pvp).HasColumnName("pvp");
                entity.Property(a => a.PrecioCompra).HasColumnName("precio_compra").HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Ubicacion>(entity =>
            {
                entity.ToTable("ubicacion");
                entity.HasKey(u => u.IdUbicacion).HasName("PRIMARY");
                entity.Property(u => u.IdUbicacion).HasColumnName("ID_UBICACION");
                entity.Property(u => u.Numero).HasColumnName("NUMERO");
                entity.Property(u => u.LetraPasillo).HasColumnName("LETRA_PASILLO").HasMaxLength(10);
                entity.Property(u => u.NumeroEstanteria).HasColumnName("NUMERO_ESTANTERIA");
                entity.Property(u => u.Cantidad).HasColumnName("CANTIDAD").HasColumnType("decimal(10,2)");
                entity.Property(u => u.IdArticulo).HasColumnName("ID_ARTICULO");
                entity.HasOne(u => u.Articulo).WithMany(a => a.Ubicaciones).HasForeignKey(u => u.IdArticulo).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Orden>(entity =>
            {
                entity.HasKey(e => e.IdOrden).HasName("PRIMARY");
                entity.ToTable("orden");
                entity.HasIndex(e => e.IdArticulo, "ID_ARTICULO");
                entity.HasIndex(e => e.IdEmpleado, "ID_EMPLEADO");
                entity.Property(e => e.IdOrden).HasColumnName("ID_ORDEN").ValueGeneratedOnAdd();
                entity.Property(e => e.Cantidad).HasColumnName("CANTIDAD");
                entity.Property(e => e.Codigo).HasMaxLength(10).HasColumnName("CODIGO");
                entity.Property(e => e.FechaFin).HasColumnType("date").HasColumnName("FECHA_FIN");
                entity.Property(e => e.IdArticulo).HasColumnName("ID_ARTICULO");
                entity.Property(e => e.IdEmpleado).HasColumnName("ID_EMPLEADO");
                entity.Property(e => e.Estado)
                      .HasColumnName("Estado")
                      .HasMaxLength(20)
                      .HasDefaultValue("Pendiente");
                entity.Ignore(e => e.EstadoEnum);
                entity.Ignore(e => e.EstadoTexto);

                entity.HasOne(d => d.IdArticuloNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.IdArticulo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("orden_ibfk_2");

                entity.HasOne(d => d.IdEmpleadoNavigation)
                    .WithMany(p => p.Ordens)
                    .HasForeignKey(d => d.IdEmpleado)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("orden_ibfk_1");
            });

            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.ToTable("empleado");
                entity.HasOne(e => e.Rol)
                      .WithMany(r => r.Empleados)
                      .HasForeignKey(e => e.IdRol);
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("pedido");
                entity.HasKey(p => p.IdPedido);
                entity.Property(p => p.IdPedido).HasColumnName("IdPedido");
                entity.Property(p => p.IdCliente).HasColumnName("IdCliente");
                entity.Property(p => p.FechaPedido).HasColumnType("date").HasColumnName("FechaPedido");
                entity.Property(p => p.FechaEntrega).HasColumnType("date").HasColumnName("FechaEntrega");
                entity.Property(p => p.Estado).HasMaxLength(20).HasColumnName("Estado").HasDefaultValue("Pendiente");
                entity.Property(p => p.Total).HasColumnType("decimal(10,2)").HasColumnName("Total");

                entity.Ignore(p => p.EstadoTexto);

                entity.HasOne(p => p.Cliente)
                      .WithMany()
                      .HasForeignKey(p => p.IdCliente)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_pedido_cliente");

                entity.HasMany(p => p.Lineas)
                      .WithOne(l => l.Pedido)
                      .HasForeignKey(l => l.IdPedido)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("fk_pedidolinea_pedido");
            });

            modelBuilder.Entity<PedidoLinea>(entity =>
            {
                entity.ToTable("pedido_linea");
                entity.HasKey(l => l.IdPedidoLinea);
                entity.Property(l => l.IdPedidoLinea).HasColumnName("IdPedidoLinea");
                entity.Property(l => l.IdPedido).HasColumnName("IdPedido");
                entity.Property(l => l.CodigoArticulo).HasMaxLength(10).HasColumnName("CodigoArticulo");
                entity.Property(l => l.Cantidad).HasColumnName("Cantidad");
                entity.Property(l => l.PrecioUnitario).HasColumnType("decimal(10,2)").HasColumnName("PrecioUnitario");

                entity.Ignore(l => l.Subtotal);

                entity.HasOne(l => l.Articulo)
                      .WithMany()
                      .HasForeignKey(l => l.CodigoArticulo)
                      .HasPrincipalKey(a => a.Codigo)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("fk_pedidolinea_articulo");
            });

            modelBuilder.Entity<ClienteHasArticulo>(entity =>
            {
                entity.HasKey(e => new { e.ClienteIdcliente, e.ArticuloCodigo });
                entity.Property(e => e.ClienteIdcliente).HasColumnName("cliente_IDCLIENTE");
                entity.Property(e => e.ArticuloCodigo).HasColumnName("articulo_CODIGO").HasMaxLength(10);

                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.ClienteHasArticulos)
                    .HasForeignKey(e => e.ClienteIdcliente);

                entity.HasOne(e => e.Articulo)
                    .WithMany(a => a.ClienteHasArticulos)
                    .HasForeignKey(e => e.ArticuloCodigo)
                    .HasPrincipalKey(a => a.Codigo);
            });

            modelBuilder.Entity<Escandallo>(entity =>
            {
                entity.ToTable("escandallo");
                entity.HasKey(e => e.IdEscandallo);
                entity.Property(e => e.IdEscandallo).HasColumnName("IdEscandallo");
                entity.Property(e => e.CodigoProducto).HasMaxLength(10).HasColumnName("CodigoProducto");
                entity.Property(e => e.Descrip).HasMaxLength(50).HasColumnName("Descrip");
                entity.Property(e => e.Descrip2).HasMaxLength(50).HasColumnName("Descrip2");
            });

            modelBuilder.Entity<ComponenteEscandallo>(entity =>
            {
                entity.ToTable("componenteescandallo");
                entity.HasKey(e => e.IdComponente);
                entity.Property(e => e.IdComponente).HasColumnName("IdComponente");
                entity.Property(e => e.IdEscandallo).HasColumnName("IdEscandallo");
                entity.Property(e => e.CodigoArticulo).HasMaxLength(10).HasColumnName("CodigoArticulo");
                entity.Property(e => e.Cantidad).HasColumnName("Cantidad");
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10,2)").HasColumnName("PrecioUnitario");
                entity.Property(e => e.CodigoComponentePadre).HasColumnName("CodigoComponentePadre");

                entity.Ignore(e => e.Descripcion);
                entity.Ignore(e => e.Descripcion2);
                entity.Ignore(e => e.Hijos);
                entity.Ignore(e => e.NombreComponente);

                entity.HasOne<Escandallo>()
                    .WithMany(p => p.Componentes)
                    .HasForeignKey(e => e.IdEscandallo)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_componenteescandallo_escandallo");
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("rol");
                entity.HasKey(r => r.Id).HasName("PRIMARY");
                entity.Property(r => r.Id).HasColumnName("ID");
                entity.Property(r => r.NombreRol).HasMaxLength(50).HasColumnName("NOMBRE_ROL");

                entity.HasMany(r => r.Permisos)
                      .WithMany(p => p.IdRols)
                      .UsingEntity(j => j.ToTable("rol_permiso")
                          .HasData()
                      );
            });

            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.ToTable("permiso");
                entity.HasKey(p => p.Id).HasName("PRIMARY");
                entity.Property(p => p.Id).HasColumnName("ID");
                entity.Property(p => p.NombrePermiso).HasMaxLength(50).HasColumnName("NOMBRE_PERMISO");
            });

            modelBuilder.Entity<Rol>()
                .HasMany(r => r.Permisos)
                .WithMany(p => p.IdRols)
                .UsingEntity<Dictionary<string, object>>(
                    "rol_permiso",
                    j => j.HasOne<Permiso>().WithMany().HasForeignKey("ID_PERMISO"),
                    j => j.HasOne<Rol>().WithMany().HasForeignKey("ID_ROL"),
                    j => j.ToTable("rol_permiso")
                );

            modelBuilder.Entity<OrdenFase>(entity =>
            {
                entity.ToTable("orden_fase");
                entity.HasKey(e => e.IdOrdenFase);
                entity.Property(e => e.IdOrdenFase).HasColumnName("IdOrdenFase");
                entity.Property(e => e.IdOrden).HasColumnName("IdOrden");
                entity.Property(e => e.CodigoFase).HasMaxLength(10).HasColumnName("CodigoFase");
                entity.Property(e => e.NumeroFase).HasColumnName("NumeroFase");
                entity.Property(e => e.CantidadEntrada).HasColumnName("CantidadEntrada");
                entity.Property(e => e.CantidadOK).HasColumnName("CantidadOK");
                entity.Property(e => e.CantidadDefecto).HasColumnName("CantidadDefecto");
                entity.Property(e => e.FechaFin).HasColumnType("date").HasColumnName("FechaFin");
                entity.Property(e => e.IdEmpleado).HasColumnName("IdEmpleado");
                entity.Property(e => e.Estado)
                      .HasMaxLength(20).HasColumnName("Estado")
                      .HasDefaultValue("Pendiente");
                entity.Ignore(e => e.EstadoEnum);
                entity.Ignore(e => e.EstadoTexto);
                entity.Ignore(e => e.NombreFase);

                entity.HasOne(e => e.OrdenNavigation)
                      .WithMany(o => o.Fases)
                      .HasForeignKey(e => e.IdOrden)
                      .HasConstraintName("fk_ordenfase_orden")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.EmpleadoNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.IdEmpleado)
                      .HasConstraintName("fk_ordenfase_emp")
                      .OnDelete(DeleteBehavior.SetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}