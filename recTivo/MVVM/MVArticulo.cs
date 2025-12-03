using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recTivo.MVVM
{
    public class MVArticulo : MVBase
    {      

        private Articulo _articulo;
        private ArticuloRepository _articuloRepository;


        public MVArticulo(ArticuloRepository articuloRepository) {
         
            _articuloRepository = articuloRepository;
            _articulo = new Articulo();
        }

    }
}
