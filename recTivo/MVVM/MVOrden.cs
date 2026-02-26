using di.proyecto.clase._2025.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.ObjectModel;

namespace recTivo.MVVM
{
    public class FilaOrdenPreview
    {
        public string CodigoArticulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Cantidad { get; set; }
        public bool EsPT { get; set; }

        public string Tipo => EsPT ? "PT" : "PS";

        // Indica si al confirmar se creará nueva o se sumará a una existente
        public bool EsNueva { get; set; } = true;
        public string AccionTexto => EsNueva ? "Nueva" : "Agrupar";
    }

    public class MVOrden : MVBase
    {
        private readonly EscandalloRepository _escandalloRepo;
        private readonly ArticuloRepository _articuloRepo;
        private readonly OrdenRepository _ordenRepo;
        private readonly EmpleadoRepository _empleadoRepo;

        public MVOrden(
            EscandalloRepository escandalloRepo,
            ArticuloRepository articuloRepo,
            OrdenRepository ordenRepo,
            EmpleadoRepository empleadoRepo)
        {
            _escandalloRepo = escandalloRepo;
            _articuloRepo = articuloRepo;
            _ordenRepo = ordenRepo;
            _empleadoRepo = empleadoRepo;
        }

        // ── Artículos PT con escandallo ───────────────────────────────────
        private List<Articulo> _articulosPT = new();
        public List<Articulo> ArticulosPT
        {
            get => _articulosPT;
            set => SetProperty(ref _articulosPT, value);
        }

        // ── Artículo PT seleccionado ──────────────────────────────────────
        private Articulo? _articuloSeleccionado;
        public Articulo? ArticuloSeleccionado
        {
            get => _articuloSeleccionado;
            set
            {
                SetProperty(ref _articuloSeleccionado, value);
                OrdenesPreview.Clear();
                PreviewVisible = false;
            }
        }

        // ── Cantidad a fabricar ───────────────────────────────────────────
        private decimal _cantidadFabricar = 1;
        public decimal CantidadFabricar
        {
            get => _cantidadFabricar;
            set => SetProperty(ref _cantidadFabricar, value);
        }

        // ── Fecha fin ─────────────────────────────────────────────────────
        private DateTime? _fechaFin;
        public DateTime? FechaFin
        {
            get => _fechaFin;
            set => SetProperty(ref _fechaFin, value);
        }

        // ── ¿Incluir el PT en las órdenes? ───────────────────────────────
        private bool _incluirPT = false;
        public bool IncluirPT
        {
            get => _incluirPT;
            set => SetProperty(ref _incluirPT, value);
        }

        // ── Preview de órdenes ────────────────────────────────────────────
        public ObservableCollection<FilaOrdenPreview> OrdenesPreview { get; } = new();

        // ── Visibilidad del panel de preview ─────────────────────────────
        private bool _previewVisible;
        public bool PreviewVisible
        {
            get => _previewVisible;
            set => SetProperty(ref _previewVisible, value);
        }

        // ── Todos los artículos en memoria ────────────────────────────────
        private List<Articulo> _todosArticulos = new();

        // ================================================================
        //   INICIALIZAR
        // ================================================================
        public async Task InicializarAsync()
        {
            try
            {
                _todosArticulos = (await _articuloRepo.GetAllAsync()).ToList();

                var escandallos = await _escandalloRepo.GetAllAsync();
                var codigosConEscandallo = new HashSet<string>(
                    escandallos.Select(e => e.CodigoProducto));

                ArticulosPT = _todosArticulos
                    .Where(a => a.Codigo.StartsWith("PT")
                             && codigosConEscandallo.Contains(a.Codigo))
                    .OrderBy(a => a.Codigo)
                    .ToList();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ÓRDENES", $"Error al inicializar: {ex.Message}");
            }
        }

        // ================================================================
        //   CALCULAR PREVIEW
        // ================================================================
        public async Task CalcularPreviewAsync()
        {
            OrdenesPreview.Clear();
            PreviewVisible = false;

            if (ArticuloSeleccionado == null)
            {
                MensajeError.Mostrar("ÓRDENES", "Selecciona un artículo PT.");
                return;
            }
            if (CantidadFabricar <= 0)
            {
                MensajeError.Mostrar("ÓRDENES", "La cantidad a fabricar debe ser mayor que 0.");
                return;
            }
            if (FechaFin == null)
            {
                MensajeError.Mostrar("ÓRDENES", "Debes seleccionar una fecha fin.");
                return;
            }

            try
            {
                var escandallo = await _escandalloRepo
                    .GetByCodigoProductoAsync(ArticuloSeleccionado.Codigo);

                if (escandallo == null)
                {
                    MensajeError.Mostrar("ÓRDENES",
                        $"El artículo '{ArticuloSeleccionado.Codigo}' no tiene escandallo.");
                    return;
                }

                var componentes = await _escandalloRepo
                    .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                // Acumular PS recursivamente
                var acumulado = new Dictionary<string, decimal>();
                await RecopilarPS(componentes, CantidadFabricar, acumulado);

                if (acumulado.Count == 0)
                {
                    MensajeError.Mostrar("ÓRDENES",
                        "Este escandallo no contiene ningún componente PS.");
                    return;
                }

                // Añadir PS al preview — comprobar si ya existe orden para marcar Agrupar
                foreach (var kvp in acumulado.OrderBy(k => k.Key))
                {
                    var art = _todosArticulos.FirstOrDefault(a => a.Codigo == kvp.Key);
                    var existente = await _ordenRepo.GetByCodigoYFechaAsync(kvp.Key, FechaFin);
                    OrdenesPreview.Add(new FilaOrdenPreview
                    {
                        CodigoArticulo = kvp.Key,
                        Descripcion = art?.descrip ?? "",
                        Cantidad = kvp.Value,
                        EsPT = false,
                        EsNueva = existente == null
                    });
                }

                // Si el usuario quiere incluir el PT, añadirlo al principio
                if (IncluirPT)
                {
                    var existentePT = await _ordenRepo
                        .GetByCodigoYFechaAsync(ArticuloSeleccionado.Codigo, FechaFin);
                    OrdenesPreview.Insert(0, new FilaOrdenPreview
                    {
                        CodigoArticulo = ArticuloSeleccionado.Codigo,
                        Descripcion = ArticuloSeleccionado.descrip ?? "",
                        Cantidad = CantidadFabricar,
                        EsPT = true,
                        EsNueva = existentePT == null
                    });
                }

                PreviewVisible = true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ÓRDENES", $"Error al calcular preview: {ex.Message}");
            }
        }

        // ================================================================
        //   RECOPILAR PS RECURSIVAMENTE
        // ================================================================
        private async Task RecopilarPS(
            List<ComponenteEscandallo> componentes,
            decimal factorCantidad,
            Dictionary<string, decimal> acumulado)
        {
            foreach (var comp in componentes)
            {
                decimal cantidadReal = (comp.Cantidad ?? 1) * factorCantidad;

                if (comp.CodigoArticulo.StartsWith("PS"))
                {
                    if (acumulado.ContainsKey(comp.CodigoArticulo))
                        acumulado[comp.CodigoArticulo] += cantidadReal;
                    else
                        acumulado[comp.CodigoArticulo] = cantidadReal;
                }
                else
                {
                    // Bajar un nivel si tiene escandallo propio
                    var subEsc = await _escandalloRepo
                        .GetByCodigoProductoAsync(comp.CodigoArticulo);

                    if (subEsc != null)
                    {
                        var subComps = await _escandalloRepo
                            .GetComponentesByEscandalloAsync(subEsc.IdEscandallo);
                        await RecopilarPS(subComps, cantidadReal, acumulado);
                    }
                }
            }
        }

        // ================================================================
        //   CONFIRMAR Y GENERAR ÓRDENES — agrupa por código + fecha
        // ================================================================
        public async Task<bool> GenerarOrdenesAsync(Empleado empleadoActual)
        {
            if (OrdenesPreview.Count == 0)
            {
                MensajeError.Mostrar("ÓRDENES",
                    "No hay órdenes que generar. Calcula el preview primero.");
                return false;
            }

            // Verificar permiso (si el rol tiene permisos configurados)
            bool tienePermiso = empleadoActual?.Rol?.Permisos
                .Any(p => p.NombrePermiso.ToLower().Contains("orden")) ?? false;

            if (!tienePermiso && (empleadoActual?.Rol?.Permisos.Count ?? 0) > 0)
            {
                MensajeError.Mostrar("ÓRDENES",
                    $"'{empleadoActual!.NombreCompleto}' no tiene permiso para generar órdenes.");
                return false;
            }

            try
            {
                int nuevas = 0;
                int agrupadas = 0;

                foreach (var fila in OrdenesPreview)
                {
                    int cantidadInt = (int)Math.Ceiling(fila.Cantidad);

                    // Buscar si ya existe una orden con mismo código y misma fecha
                    var ordenExistente = await _ordenRepo
                        .GetByCodigoYFechaAsync(fila.CodigoArticulo, FechaFin);

                    if (ordenExistente != null)
                    {
                        // Agrupar: sumar cantidad
                        ordenExistente.Cantidad += cantidadInt;
                        await _ordenRepo.UpdateAsync(ordenExistente);
                        agrupadas++;
                    }
                    else
                    {
                        // Crear nueva
                        var articuloId = _todosArticulos
                            .FirstOrDefault(a => a.Codigo == fila.CodigoArticulo)?.IdArticulo ?? 0;

                        var nuevaOrden = new Orden
                        {
                            Codigo = fila.CodigoArticulo,
                            Cantidad = cantidadInt,
                            FechaFin = FechaFin,
                            IdEmpleado = empleadoActual!.Id,
                            IdArticulo = articuloId,
                            Estado = nameof(EstadoOrden.Pendiente)
                        };

                        await _ordenRepo.AddAsync(nuevaOrden);
                        nuevas++;
                    }
                }

                string resumen = $"Órdenes generadas: {nuevas} nuevas";
                if (agrupadas > 0)
                    resumen += $", {agrupadas} agrupadas con órdenes existentes";
                resumen += ".";

                MensajeInformacion.Mostrar("ÓRDENES", resumen, 2);

                // Reset
                ArticuloSeleccionado = null;
                CantidadFabricar = 1;
                FechaFin = null;
                IncluirPT = false;
                OrdenesPreview.Clear();
                PreviewVisible = false;
                OnPropertyChanged(nameof(IncluirPT));

                return true;
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ÓRDENES", $"Error al guardar las órdenes: {ex.Message}");
                return false;
            }
        }
    }
}
