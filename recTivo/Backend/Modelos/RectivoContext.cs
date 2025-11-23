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
        public virtual DbSet<Ubicacion> Ubicacions { get; set; } = null!;
        public virtual DbSet<ClienteHasArticulo> ClienteHasArticulos { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var conn = "server=localhost;database=RECTIVO;user=root;password=mysql;";
            optionsBuilder.UseMySQL(conn);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de Orden
            modelBuilder.Entity<Orden>(entity =>
            {
                entity.HasKey(e => e.IdOrden).HasName("PRIMARY");

                entity.ToTable("orden");

                entity.HasIndex(e => e.IdArticulo, "ID_ARTICULO");
                entity.HasIndex(e => e.IdEmpleado, "ID_EMPLEADO");

                entity.Property(e => e.IdOrden).HasColumnName("ID_ORDEN");
                entity.Property(e => e.Cantidad).HasColumnName("CANTIDAD");
                entity.Property(e => e.Codigo)
                    .HasMaxLength(10)
                    .HasColumnName("CODIGO");
                entity.Property(e => e.FechaFin)
                    .HasColumnType("date")
                    .HasColumnName("FECHA_FIN");
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

            modelBuilder.Entity<Empleado>().ToTable("empleado");
            modelBuilder.Entity<Empleado>()
                        .HasOne(e => e.Rol)
                        .WithMany(r => r.Empleados)
                        .HasForeignKey(e => e.IdRol);


            // Configuración de ClienteHasArticulo (clave compuesta + relaciones)
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

            OnModelCreatingPartial(modelBuilder);
        }
    

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
