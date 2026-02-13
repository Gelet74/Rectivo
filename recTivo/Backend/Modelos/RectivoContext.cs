using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace recTivo.Backend.Modelos
{
    public partial class RectivoContext : DbContext
    {
        public RectivoContext(DbContextOptions<RectivoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<VistaArticuloUbicacion> VistaArticulosUbicacions { get; set; } = null!;
        public virtual DbSet<Articulo> Articulos { get; set; } = null!;
        public virtual DbSet<Cliente> Clientes { get; set; } = null!;
        public virtual DbSet<Empleado> Empleados { get; set; } = null!;
        public virtual DbSet<Orden> Ordens { get; set; } = null!;
        public virtual DbSet<Permiso> Permisos { get; set; } = null!;
        public virtual DbSet<Rol> Rols { get; set; } = null!;
        public virtual DbSet<Ubicacion> Ubicacion { get; set; } = null!;
        public virtual DbSet<ClienteHasArticulo> ClienteHasArticulos { get; set; } = null!;
        public virtual DbSet<Escandallo> Escandallos { get; set; } = null!;
        public virtual DbSet<ComponenteEscandallo> ComponenteEscandallos { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conn = "server=localhost;database=RECTIVO;user=root;password=mysql;";
                optionsBuilder.UseMySQL(conn);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===========================
            // CONFIGURACIÓN DE CLIENTE
            // ===========================
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("cliente");

                entity.HasKey(c => c.IdCliente).HasName("PRIMARY");

                entity.Property(c => c.IdCliente)
                      .HasColumnName("IDCLIENTE");

                entity.Property(c => c.Nombre)
                      .HasColumnName("NOMBRE")
                      .HasMaxLength(50);

                entity.Property(c => c.Apellido1)
                      .HasColumnName("APELLIDO1")
                      .HasMaxLength(50);

                entity.Property(c => c.Apellido2)
                      .HasColumnName("APELLIDO2")
                      .HasMaxLength(50);

                entity.Property(c => c.NumFactura)
                      .HasColumnName("NUM_FACTURA");

                entity.Property(c => c.NumPedido)
                      .HasColumnName("NUM_PEDIDO");

                entity.Property(c => c.Dni)
                      .HasColumnName("DNI")
                      .HasMaxLength(20);

                entity.Property(c => c.Telefono)
                      .HasColumnName("TELEFONO")
                      .HasMaxLength(20);

                entity.Property(c => c.Usuario)
                      .HasColumnName("username")
                      .HasMaxLength(50);

                entity.Property(c => c.Password)
                      .HasColumnName("password")
                      .HasMaxLength(255);
            });

            // ===========================
            // CONFIGURACIÓN DE ARTÍCULO
            // ===========================
            modelBuilder.Entity<Articulo>(entity =>
            {
                entity.ToTable("articulo");

                entity.HasKey(a => a.IdArticulo).HasName("PRIMARY");

                entity.Property(a => a.IdArticulo).HasColumnName("id_articulo");

                entity.Property(a => a.Codigo)
                      .HasColumnName("codigo")
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(a => a.descrip)
                      .HasColumnName("descripcion")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(a => a.descrip2)
                      .HasColumnName("descripcion2")
                      .HasMaxLength(50);

                entity.Property(a => a.Stock)
                      .HasColumnName("stock")
                      .HasDefaultValue(0);

                entity.Property(a => a.Pvp)
                      .HasColumnName("pvp");

                entity.Property(a => a.IdUbicacion)
                      .HasColumnName("id_ubicacion");

                entity.HasOne(a => a.Ubicacion)
                      .WithMany()
                      .HasForeignKey(a => a.IdUbicacion)
                      .HasPrincipalKey(u => u.IdUbicacion)
                      .HasConstraintName("FK_articulo_ubicacion");
            });


            // ===========================
            // CONFIGURACIÓN DE ORDEN
            // ===========================
            modelBuilder.Entity<Orden>(entity =>
            {
                entity.HasKey(e => e.IdOrden).HasName("PRIMARY");

                entity.ToTable("orden");

                entity.HasIndex(e => e.IdArticulo, "ID_ARTICULO");
                entity.HasIndex(e => e.IdEmpleado, "ID_EMPLEADO");

                entity.Property(e => e.IdOrden).HasColumnName("ID_ORDEN");
                entity.Property(e => e.Cantidad).HasColumnName("CANTIDAD");
                entity.Property(e => e.Codigo).HasMaxLength(10).HasColumnName("CODIGO");
                entity.Property(e => e.FechaFin).HasColumnType("date").HasColumnName("FECHA_FIN");
                entity.Property(e => e.IdArticulo).HasColumnName("ID_ARTICULO");
                entity.Property(e => e.IdEmpleado).HasColumnName("ID_EMPLEADO");

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

            // ===========================
            // CONFIGURACIÓN DE EMPLEADO
            // ===========================
            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.ToTable("empleado");

                entity.HasOne(e => e.Rol)
                      .WithMany(r => r.Empleados)
                      .HasForeignKey(e => e.IdRol);
            });

            modelBuilder.Entity<Ubicacion>(entity =>
            {
                entity.ToTable("ubicacion");

                entity.HasKey(u => u.IdUbicacion).HasName("PRIMARY");

                entity.Property(u => u.IdUbicacion).HasColumnName("ID_UBICACION");

                entity.Property(u => u.Numero).HasColumnName("NUMERO");
                entity.Property(u => u.LetraPasillo).HasColumnName("LETRA_PASILLO").HasMaxLength(10);
                entity.Property(u => u.NumeroEstanteria).HasColumnName("NUMERO_ESTANTERIA");

                entity.HasMany(u => u.Articulos)
                      .WithOne(a => a.Ubicacion)
                      .HasForeignKey(a => a.IdUbicacion)
                      .IsRequired(false)
                      .HasConstraintName("FK_articulo_ubicacion");
            });


            // ===========================
            // CONFIGURACIÓN DE CLIENTE_HAS_ARTICULO
            // ===========================
            modelBuilder.Entity<ClienteHasArticulo>(entity =>
            {
                entity.HasKey(e => new { e.ClienteIdcliente, e.ArticuloIdArticulo });

                entity.Property(e => e.ClienteIdcliente).HasColumnName("cliente_IDCLIENTE");
                entity.Property(e => e.ArticuloIdArticulo).HasColumnName("articulo_IDARTICULO");

                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.ClienteHasArticulos)
                    .HasForeignKey(e => e.ClienteIdcliente);

                entity.HasOne(e => e.Articulo)
                    .WithMany(a => a.ClienteHasArticulos)
                    .HasForeignKey(e => e.ArticuloIdArticulo);
            });

            // ===========================
            // CONFIGURACIÓN DE ESCANDALLO
            // ===========================
            modelBuilder.Entity<Escandallo>(entity =>
            {
                entity.ToTable("escandallo");

                entity.HasKey(e => e.IdEscandallo);

                entity.Property(e => e.IdEscandallo).HasColumnName("IdEscandallo");
                entity.Property(e => e.CodigoProducto).HasMaxLength(10).HasColumnName("CodigoProducto");
                entity.Property(e => e.Descrip).HasMaxLength(50).HasColumnName("Descrip");
                entity.Property(e => e.Descrip2).HasMaxLength(50).HasColumnName("Descrip2");
            });

            // ===========================
            // CONFIGURACIÓN DE COMPONENTE ESCANDALLO
            // ===========================
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


                entity.HasOne(e => e.Escandallo)
                    .WithMany(p => p.Componentes)
                    .HasForeignKey(e => e.IdEscandallo)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_componenteescandallo_escandallo");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
