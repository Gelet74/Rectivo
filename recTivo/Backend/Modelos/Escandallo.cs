using recTivo.MVVM.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos
{
    public class Escandallo : ValidatableViewModel
    {
        public int IdEscandallo { get; set; }
        public string CodigoProducto { get; set; }
        public string Descrip { get; set; }
        [Column("descrip2")]
        public string Descrip2 { get; set; }

        public ICollection<ComponenteEscandallo> Componentes { get; set; } = new List<ComponenteEscandallo>();
    }
}
