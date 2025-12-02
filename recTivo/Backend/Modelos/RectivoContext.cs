using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace recTivo.Backend.Modelos
{
    public partial class RectivoContext : DbContext
    {
        public RectivoContext()
        {
        }

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var conn = "server=localhost;database=RECTIVO;user=root;password=mysql;";
            optionsBuilder.UseMySQL(conn);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
            modelBuilder.Entity<Empleado>().ToTable("empleado");

            modelBuilder.Entity<Empleado>()
                        .HasOne(e => e.Rol)
                        .WithMany(r => r.Empleados)
                        .HasForeignKey(e => e.IdRol);

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
            // CONFIGURACIÓN DE ESCANDALLO (NUEVO)
            // ===========================
            modelBuilder.Entity<Escandallo>(entity =>
            {
                entity.ToTable("escandallo");

                entity.HasKey(e => e.IdEscandallo);

                entity.Property(e => e.IdEscandallo).HasColumnName("IdEscandallo");
                entity.Property(e => e.CodigoProducto).HasMaxLength(10).HasColumnName("CodigoProducto");
                entity.Property(e => e.NombreProducto).HasMaxLength(50).HasColumnName("NombreProducto");
                entity.Property(e => e.Descripcion2).HasMaxLength(50).HasColumnName("Descripcion2");
            });


            // ===========================
            // CONFIGURACIÓN DE COMPONENTE ESCANDALLO (NUEVO)
            // ===========================
            modelBuilder.Entity<ComponenteEscandallo>(entity =>
            {
                entity.ToTable("componenteescandallo");

                entity.HasKey(e => e.IdComponente);

                entity.Property(e => e.IdComponente).HasColumnName("IdComponente");
                entity.Property(e => e.IdEscandallo).HasColumnName("IdEscandallo");
                entity.Property(e => e.CodigoArticulo).HasMaxLength(10).HasColumnName("CodigoArticulo");
                entity.Property(e => e.Descripcion).HasMaxLength(50).HasColumnName("Descripcion");
                entity.Property(e => e.Descripcion2).HasMaxLength(50).HasColumnName("Descripcion2");
                entity.Property(e => e.Cantidad).HasColumnName("Cantidad");
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10,2)").HasColumnName("PrecioUnitario");

                entity.HasOne(e => e.Escandallo)
                    .WithMany(p => p.Componentes)
                    .HasForeignKey(e => e.IdEscandallo)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_componenteescandallo_escandallo");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        public virtual DbSet<Escandallo> Escandallos { get; set; } = null!;
        public virtual DbSet<ComponenteEscandallo> ComponenteEscandallos { get; set; } = null!;



        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
