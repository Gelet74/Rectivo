using recTivo.MVVM.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace recTivo.Backend.Modelos
{
    [Table("escandallo")] 
    public class Escandallo : ValidatableViewModel
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEscandallo { get; set; }

        [Required]
        [StringLength(10)]
        public string CodigoProducto { get; set; } = null!;

        [Required]
        [StringLength(50)]
        [Column("Descrip")] 
        public string Descrip { get; set; } = null!;

        [StringLength(50)]
        [Column("Descrip2")]
        public string? Descrip2 { get; set; }

        public virtual ICollection<ComponenteEscandallo> Componentes { get; set; } = new List<ComponenteEscandallo>();
    }
}