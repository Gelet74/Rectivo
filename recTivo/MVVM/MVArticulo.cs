using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System;
using System.Threading.Tasks;

namespace recTivo.MVVM
{
    public class MVArticulo : MVBase
    {
        private readonly ArticuloRepository _articuloRepository;
        private Articulo _articulo;

        public MVArticulo(ArticuloRepository articuloRepository)
        {
            _articuloRepository = articuloRepository;
            _articulo = new Articulo();
        }

        // Propiedades para enlazar en XAML
        public string Codigo
        {
            get => _articulo.Codigo;
            set { _articulo.Codigo = value; OnPropertyChanged(); }
        }

        public string Descrip
        {
            get => _articulo.Descrip;
            set { _articulo.Descrip = value; OnPropertyChanged(); }
        }

        public string Descrip2
        {
            get => _articulo.Descrip2;
            set { _articulo.Descrip2 = value; OnPropertyChanged(); }
        }

        public double? Pvp
        {
            get => _articulo.Pvp;
            set { _articulo.Pvp = value; OnPropertyChanged(); }
        }

        public async Task<bool> BajaPorCodigoAsync(string codigo)
        {
            try
            {
                var articulo = await _articuloRepository.GetByCodigoAsync(codigo);
                if (articulo != null)
                {
                    _articuloRepository.Remove(articulo);
                    await _articuloRepository.SaveChangesAsync(); // 👈 persistimos
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error al dar de baja artículo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GuardarAsync()
        {
            try
            {
                await _articuloRepository.AddAsync(_articulo);
                await _articuloRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                SnackbarMessageQueue.Enqueue($"Error al guardar artículo: {ex.Message}");
                return false;
            }
        }
    }
}
