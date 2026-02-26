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

                // CORREGIDO: validar Estanteria y Hueco antes de parsear
                if (!int.TryParse(Estanteria, out int estanteria) || !int.TryParse(Hueco, out int hueco))
                {
                    MensajeAdvertencia.Mostrar("AVISO", "Estantería y hueco deben ser números válidos.");
                    return;
                }

                var ubicacion = await _context.Ubicacion.FirstOrDefaultAsync(u =>
                    u.LetraPasillo == Pasillo &&
                    u.NumeroEstanteria == estanteria &&
                    u.Numero == hueco);

                if (ubicacion == null)
                {
                    ubicacion = new Ubicacion
                    {
                        LetraPasillo = Pasillo,
                        NumeroEstanteria = estanteria,
                        Numero = hueco,
                        IdArticulo = articulo.IdArticulo,
                        Cantidad = cantidadAIngresar
                    };
                    _context.Ubicacion.Add(ubicacion);
                }
                else
                {
                    // CORREGIDO: avisar si la ubicación ya tiene un artículo diferente
                    if (ubicacion.IdArticulo != null && ubicacion.IdArticulo != articulo.IdArticulo)
                    {
                        MensajeError.Mostrar("ERROR", "Esa ubicación ya contiene un artículo diferente.");
                        return;
                    }

                    ubicacion.IdArticulo = articulo.IdArticulo;
                    ubicacion.Cantidad += cantidadAIngresar;
                    _context.Ubicacion.Update(ubicacion);
                }

                await _context.SaveChangesAsync();

                // CORREGIDO: recalcular Stock como suma real de ubicaciones
                articulo.Stock = await _context.Ubicacion
                    .Where(u => u.IdArticulo == articulo.IdArticulo)
                    .SumAsync(u => u.Cantidad);

                _context.Articulos.Update(articulo);
                await _context.SaveChangesAsync();

                MensajeInformacion.Mostrar("ÉXITO", $"Stock actualizado. Stock total: {articulo.Stock}");
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

                if (!int.TryParse(Estanteria, out int estanteria) || !int.TryParse(Hueco, out int hueco))
                {
                    MensajeAdvertencia.Mostrar("AVISO", "Estantería y hueco deben ser números válidos.");
                    return;
                }

                // Buscar la ubicación concreta indicada por el usuario
                var ubicacion = await _context.Ubicacion.FirstOrDefaultAsync(u =>
                    u.LetraPasillo == Pasillo &&
                    u.NumeroEstanteria == estanteria &&
                    u.Numero == hueco &&
                    u.IdArticulo == articulo.IdArticulo);

                if (ubicacion == null)
                {
                    MensajeError.Mostrar("ERROR", "No existe esa ubicación para el artículo seleccionado.");
                    return;
                }

                if (ubicacion.Cantidad < cantidadASacar)
                {
                    MensajeError.Mostrar("ERROR", $"Stock insuficiente en esa ubicación. Disponible: {ubicacion.Cantidad}");
                    return;
                }

                ubicacion.Cantidad -= cantidadASacar;

                if (ubicacion.Cantidad == 0)
                    ubicacion.IdArticulo = null;

                _context.Ubicacion.Update(ubicacion);
                await _context.SaveChangesAsync();

                // Recalcular Stock como suma real de todas las ubicaciones del artículo
                articulo.Stock = await _context.Ubicacion
                    .Where(u => u.IdArticulo == articulo.IdArticulo)
                    .SumAsync(u => u.Cantidad);

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
            MVArticulo.LimpiarFiltros();
        }
    }
}