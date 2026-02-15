using recTivo.MVVM.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace recTivo.Backend.Modelos
{
    public class Articulo : ValidatableViewModel
    {
        public int IdArticulo { get; set; }

        public string Codigo { get; set; } = null!;

        [Column("descripcion")]
        public string descrip { get; set; } = null!;

        [Column("descripcion2")]
        public string? descrip2 { get; set; }

        public int? Stock { get; set; } = 0;

        public double? Pvp { get; set; }

        [Column("precio_compra")]
        public decimal? PrecioCompra { get; set; }

        // ===========================
        // RELACIONES
        // ===========================
        public virtual ICollection<ArticuloUbicacion> ArticuloUbicaciones { get; set; } = new List<ArticuloUbicacion>();

        // Relación con ClienteHasArticulo
        public virtual ICollection<ClienteHasArticulo> ClienteHasArticulos { get; set; } = new List<ClienteHasArticulo>();

        // ===========================
        // PROPIEDADES CALCULADAS
        // ===========================

        [NotMapped]
        public string UbicacionesResumen
        {
            get
            {
                if (ArticuloUbicaciones == null || !ArticuloUbicaciones.Any())
                    return "-";

                return string.Join(" | ", ArticuloUbicaciones.Select(u =>
                    $"{u.Ubicacion?.LetraPasillo ?? "?"}-{u.Ubicacion?.NumeroEstanteria?.ToString() ?? "?"}-{u.Ubicacion?.Numero?.ToString() ?? "?"} ({u.Cantidad})"));
            }
        }

        [NotMapped]
        public int StockTotal
        {
            get
            {
                return ArticuloUbicaciones?.Sum(u => u.Cantidad) ?? Stock ?? 0;
            }
        }
    }
}