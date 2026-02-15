using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using recTivo.MVVM.Base;

namespace recTivo.MVVM
{
    public class MVAlmacen : MVBase
    {
        private readonly RectivoContext _context;
        public MVArticulo MVArticulo { get; }

        public MVAlmacen(RectivoContext context, MVArticulo mvArticulo)
        {
            _context = context;
            MVArticulo = mvArticulo;
        }

        // Propiedades de la UI (Binding)
        private string _cantidad;
        public string Cantidad { get => _cantidad; set => SetProperty(ref _cantidad, value); }

        private string _pasillo;
        public string Pasillo { get => _pasillo; set => SetProperty(ref _pasillo, value); }

        private string _estanteria;
        public string Estanteria { get => _estanteria; set => SetProperty(ref _estanteria, value); }

        private string _hueco;
        public string Hueco { get => _hueco; set => SetProperty(ref _hueco, value); }

        // -----------------------------
        // MÉTODO: ENTRADA ALMACÉN
        // -----------------------------
        public async Task AñadirAlmacen()
        {
            try
            {
                var articulo = MVArticulo.ArticuloSeleccionado;
                if (articulo == null) return;

                if (!int.TryParse(Cantidad, out int cantidadAIngresar) || cantidadAIngresar <= 0) return;

                // 1. Buscar si la ubicación física existe en la tabla "Ubicaciones"
                var ubicacion = await _context.Ubicacion.FirstOrDefaultAsync(u =>
                    u.LetraPasillo == Pasillo &&
                    u.NumeroEstanteria == int.Parse(Estanteria) &&
                    u.Numero == int.Parse(Hueco));

                if (ubicacion == null)
                {
                    // Creamos una nueva fila en la tabla ubicacion vinculada al artículo
                    ubicacion = new Ubicacion
                    {
                        LetraPasillo = Pasillo,
                        NumeroEstanteria = int.Parse(Estanteria),
                        Numero = int.Parse(Hueco),
                        IdArticulo = articulo.IdArticulo,
                        Cantidad = cantidadAIngresar
                    };
                    _context.Ubicacion.Add(ubicacion);
                }
                else
                {
                    ubicacion.IdArticulo = articulo.IdArticulo;
                    ubicacion.Cantidad += cantidadAIngresar;
                    _context.Ubicacion.Update(ubicacion);
                }

                articulo.Stock += cantidadAIngresar;
                _context.Articulos.Update(articulo);

                await _context.SaveChangesAsync();
                MensajeInformacion.Mostrar("ÉXITO", "Stock actualizado en ubicación.");
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR", ex.Message);
            }
        }

        // -----------------------------
        // MÉTODO: SALIDA ALMACÉN
        // -----------------------------
        public async Task SalidaAlmacen()
        {
            try
            {
                var articulo = MVArticulo.ArticuloSeleccionado;
                if (articulo == null)
                {
                    MensajeError.Mostrar("ERROR", "Debes seleccionar un artículo válido.");
                    return;
                }

                if (!int.TryParse(Cantidad, out int cantidadASacar) || cantidadASacar <= 0)
                {
                    MensajeAdvertencia.Mostrar("AVISO", "Introduce una cantidad válida.");
                    return;
                }

                var ubicacion = await _context.Ubicacion
                    .FirstOrDefaultAsync(u => u.IdArticulo == articulo.IdArticulo && u.Cantidad >= cantidadASacar);

                if (ubicacion == null)
                {
                    MensajeError.Mostrar("ERROR", "No hay ninguna ubicación con stock suficiente para retirar esa cantidad.");
                    return;
                }

                ubicacion.Cantidad -= cantidadASacar;

                if (ubicacion.Cantidad <= 0)
                {
                    ubicacion.IdArticulo = null;
                    ubicacion.Cantidad = 0;
                }

                articulo.Stock -= cantidadASacar;

                _context.Ubicacion.Update(ubicacion);
                _context.Articulos.Update(articulo);

                await _context.SaveChangesAsync();

                MensajeInformacion.Mostrar("ÉXITO",
                    $"Se retiraron {cantidadASacar} unidades. Stock total restante: {articulo.Stock}");

                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR", $"Error al retirar del almacén: {ex.Message}");
            }
        }

        private void LimpiarCampos()
        {
            Cantidad = ""; Pasillo = ""; Estanteria = ""; Hueco = "";
            MVArticulo.ArticuloSeleccionado = null;
        }
    }
}