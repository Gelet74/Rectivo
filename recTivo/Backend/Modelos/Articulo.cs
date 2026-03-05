using recTivo.MVVM.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace recTivo.Backend.Modelos
{
    public class Articulo : ValidatableViewModel, IDataErrorInfo
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

        // Un artículo puede estar en múltiples ubicaciones físicas
        public virtual ICollection<Ubicacion> Ubicaciones { get; set; } = new List<Ubicacion>();

        public virtual ICollection<ClienteHasArticulo> ClienteHasArticulos { get; set; } = new List<ClienteHasArticulo>();

        // ===========================
        // PROPIEDADES CALCULADAS
        // ===========================

        [NotMapped]
        public int StockTotal => Ubicaciones?.Sum(u => u.Cantidad) ?? Stock ?? 0;

        [NotMapped]
        public string UbicacionesResumen
        {
            get
            {
                if (Ubicaciones == null || !Ubicaciones.Any())
                    return "-";
                return string.Join(" | ", Ubicaciones.Select(u =>
                    $"{u.LetraPasillo ?? "?"}-{u.NumeroEstanteria?.ToString() ?? "?"}-{u.Numero?.ToString() ?? "?"} ({u.Cantidad})"));
            }
        }
        // ===========================
        // VALIDACIONES (IDataErrorInfo)
        // ===========================

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                return columnName switch
                {
                    nameof(Codigo) when string.IsNullOrWhiteSpace(Codigo)
                        => "El código es obligatorio.",
                    nameof(descrip) when string.IsNullOrWhiteSpace(descrip)
                        => "La descripción es obligatoria.",
                    _ => string.Empty
                };
            }
        }

        /// <summary>Devuelve true si todos los campos obligatorios son válidos.</summary>
        [NotMapped]
        public bool EsValido =>
            !string.IsNullOrWhiteSpace(Codigo) &&
            !string.IsNullOrWhiteSpace(descrip);
    }
}