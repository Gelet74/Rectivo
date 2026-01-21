using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.EntityFrameworkCore;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.ObjectModel;

namespace recTivo.MVVM
{
    public class MVEscandallo : MVBase
    {
        private readonly EscandalloRepository _escandalloRepository;
        private readonly ArticuloRepository _articuloRepository;
        private readonly OrdenRepository _ordenRepository;

        public MVEscandallo(
            EscandalloRepository escandalloRepository,
            ArticuloRepository articuloRepository,
            OrdenRepository ordenRepository)
        {
            _escandalloRepository = escandalloRepository;
            _articuloRepository = articuloRepository;
            _ordenRepository = ordenRepository;

            ArticuloFinal = new Articulo();
            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
        }

        // ============================================================
        //   PROPIEDADES
        // ============================================================

        public ObservableCollection<ComponenteEscandallo> Componentes { get; set; } = new();
        public ObservableCollection<Articulo> ArticulosPT { get; set; } = new();
        public ObservableCollection<Articulo> ArticulosNoPT { get; set; } = new();
        public ObservableCollection<ComponenteEscandallo> EscandalloActual { get; set; } = new();

        private List<Articulo> _listaArticulos;
        public List<Articulo> ListaArticulos
        {
            get => _listaArticulos;
            set => SetProperty(ref _listaArticulos, value);
        }




        public List<string> CodigosArticulos { get; set; }

        public string DescripcionFinal => ArticuloFinal?.Descrip ?? "";
        public string Descripcion2Final => ArticuloFinal?.Descrip2 ?? "";
        public string DescripcionArticulo { get; set; }

        private Articulo _articuloFinal;
        public Articulo ArticuloFinal
        {
            get => _articuloFinal;
            set
            {
                SetProperty(ref _articuloFinal, value);
                OnPropertyChanged(nameof(DescripcionFinal));
                OnPropertyChanged(nameof(Descripcion2Final));
            }
        }

        private ComponenteEscandallo _componenteNuevo;
        public ComponenteEscandallo ComponenteNuevo
        {
            get => _componenteNuevo;
            set => SetProperty(ref _componenteNuevo, value);
        }

        private ComponenteEscandallo _componentePadreSeleccionado;
        public ComponenteEscandallo ComponentePadreSeleccionado
        {
            get => _componentePadreSeleccionado;
            set => SetProperty(ref _componentePadreSeleccionado, value);
        }

        private ComponenteEscandallo _componenteSeleccionado;
        public ComponenteEscandallo ComponenteSeleccionado
        {
            get => _componenteSeleccionado;
            set => SetProperty(ref _componenteSeleccionado, value);
        }

        private Articulo _articuloSeleccionado;
        public Articulo ArticuloSeleccionado
        {
            get => _articuloSeleccionado;
            set
            {
                SetProperty(ref _articuloSeleccionado, value);
                CodigoSeleccionado = value?.Codigo;
            }
        }

        public string CodigoSeleccionado { get; set; }

        // ============================================================
        //   INICIALIZACIÓN
        // ============================================================

        public async Task Inicializa()
        {
            try
            {
                var lista = await _articuloRepository.GetAllAsync();

                ArticulosPT.Clear();
                ArticulosNoPT.Clear();

                foreach (var a in lista.OrderBy(a => a.Codigo))
                {
                    if (a.Codigo.StartsWith("PT"))
                        ArticulosPT.Add(a);
                    else
                        ArticulosNoPT.Add(a);
                }

                ListaArticulos = lista.ToList();
                CodigosArticulos = await _articuloRepository.Query(true).Select(a => a.Codigo).ToListAsync();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR", $"Error al inicializar: {ex.Message}");
            }
        }

        // ============================================================
        //   AÑADIR COMPONENTE RAÍZ
        // ============================================================

        public void AñadirComponente()
        {
            if (ArticuloFinal == null || string.IsNullOrWhiteSpace(ArticuloFinal.Codigo))
            {
                MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un artículo válido.");
                return;
            }

            // NO PERMITIR PT COMO RAÍZ
            if (ArticuloFinal.Codigo.StartsWith("PT"))
            {
                MensajeError.Mostrar("ESCANDALLO",
                    "No puedes crear un escandallo para un artículo PT. Selecciona un componente.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ComponenteNuevo.CodigoArticulo) || ComponenteNuevo.Cantidad <= 0)
            {
                MensajeError.Mostrar("ESCANDALLO", "Código o cantidad inválidos.");
                return;
            }

            if (ComponenteNuevo.CodigoArticulo.StartsWith("PT"))
            {
                MensajeError.Mostrar("ESCANDALLO", "Un artículo PT no puede ser componente.");
                return;
            }

            var articulo = ArticulosNoPT.FirstOrDefault(a => a.Codigo == ComponenteNuevo.CodigoArticulo);

            Componentes.Add(new ComponenteEscandallo
            {
                CodigoArticulo = ComponenteNuevo.CodigoArticulo,
                Cantidad = ComponenteNuevo.Cantidad,
                Descripcion = articulo?.Descrip ?? "",
                Descripcion2 = articulo?.Descrip2 ?? "",
                Hijos = new ObservableCollection<ComponenteEscandallo>()
            });

            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
            OnPropertyChanged(nameof(ComponenteNuevo));
        }

        // ============================================================
        //   AÑADIR SUBCOMPONENTE
        // ============================================================

        public void AñadirSubcomponente()
        {
            if (ComponentePadreSeleccionado == null)
            {
                MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un componente padre en el árbol.");
                return;
            }

            // NO PERMITIR HIJOS DE PT
            if (ComponentePadreSeleccionado.CodigoArticulo.StartsWith("PT"))
            {
                MensajeError.Mostrar("ESCANDALLO", "No puedes añadir hijos a un artículo PT.");
                return;
            }

            if (string.IsNullOrWhiteSpace(ComponenteNuevo.CodigoArticulo) || ComponenteNuevo.Cantidad <= 0)
            {
                MensajeError.Mostrar("ESCANDALLO", "Código o cantidad inválidos.");
                return;
            }

            if (ComponenteNuevo.CodigoArticulo.StartsWith("PT"))
            {
                MensajeError.Mostrar("ESCANDALLO", "Un artículo PT no puede ser hijo.");
                return;
            }

            var articulo = ArticulosNoPT.FirstOrDefault(a => a.Codigo == ComponenteNuevo.CodigoArticulo);

            ComponentePadreSeleccionado.Hijos.Add(new ComponenteEscandallo
            {
                CodigoArticulo = ComponenteNuevo.CodigoArticulo,
                Cantidad = ComponenteNuevo.Cantidad,
                Descripcion = articulo?.Descrip ?? "",
                Descripcion2 = articulo?.Descrip2 ?? "",
                CodigoComponentePadre = ComponentePadreSeleccionado.CodigoArticulo,
                Hijos = new ObservableCollection<ComponenteEscandallo>()
            });

            OnPropertyChanged(nameof(Componentes));

            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
            OnPropertyChanged(nameof(ComponenteNuevo));
        }

        // ============================================================
        //   GUARDAR ESCANDALLO
        // ============================================================

        public async Task GuardarEscandallo()
        {
            try
            {
                // ================================
                // VALIDAR SELECCIÓN EN TREEVIEW
                // ================================
                if (ComponenteSeleccionado == null)
                {
                    MensajeError.Mostrar("ESCANDALLO",
                        "Debes seleccionar un componente del árbol para crear su escandallo.");
                    return;
                }

                // No permitir escandallos de PT
                if (ComponenteSeleccionado.CodigoArticulo.StartsWith("PT"))
                {
                    MensajeError.Mostrar("ESCANDALLO",
                        "Los artículos PT ya tienen escandallo. Selecciona un componente.");
                    return;
                }

                // ================================
                // ELIMINAR ESCANDALLO EXISTENTE
                // ================================
                var existente = await _escandalloRepository
                    .GetByCodigoProductoAsync(ComponenteSeleccionado.CodigoArticulo);

                if (existente != null)
                    await _escandalloRepository.DeleteByIdAsync(existente.IdEscandallo);

                // ================================
                // CREAR NUEVO ESCANDALLO
                // ================================
                var nuevoEsc = new Escandallo
                {
                    CodigoProducto = ComponenteSeleccionado.CodigoArticulo,
                    NombreProducto = ComponenteSeleccionado.Descripcion,
                    Descripcion2 = ComponenteSeleccionado.Descripcion2
                };

                await _escandalloRepository.AddAsync(nuevoEsc);

                // ================================
                // GUARDAR HIJOS DEL COMPONENTE
                // ================================
                foreach (var hijo in ComponenteSeleccionado.Hijos)
                    await GuardarComponenteRecursivo(hijo, nuevoEsc.IdEscandallo, null);

                // ================================
                // FINALIZAR
                // ================================
                MensajeInformacion.Mostrar("ESCANDALLO",
                    "Escandallo del componente guardado correctamente.", 1);

                ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
                OnPropertyChanged(nameof(ComponenteNuevo));
            }
            catch (Exception ex)
            {
                var detalle = ex.InnerException?.Message ?? ex.Message;
                MensajeError.Mostrar("ESCANDALLO", $"Error al guardar (detalle): {detalle}");
            }
        }


        // ============================================================
        //   GUARDADO RECURSIVO
        // ============================================================

        private async Task GuardarComponenteRecursivo(
            ComponenteEscandallo comp,
            int idEscandallo,
            string padre)
        {
            // ================================
            // VALIDAR ARTÍCULO
            // ================================
            var articulo = await _articuloRepository.GetByCodigoAsync(comp.CodigoArticulo);

            // Si no existe en la tabla ARTICULO, intentamos recuperarlo del árbol
            if (articulo == null)
            {
                // comp ya tiene Descripcion, Descripcion2 y PrecioUnitario
                articulo = new Articulo
                {
                    Codigo = comp.CodigoArticulo,
                    Descrip = comp.Descripcion ?? "SIN DESCRIPCIÓN",
                    Descrip2 = comp.Descripcion2,
                    PrecioCompra = comp.PrecioUnitario
                };
            }

            // ================================
            // VALIDAR CANTIDAD
            // ================================
            var cantidad = comp.Cantidad ?? 0;
            if (cantidad <= 0)
            {
                MensajeError.Mostrar("ESCANDALLO",
                    $"Cantidad inválida para '{comp.CodigoArticulo}'.");
                return;
            }

            // ================================
            // CREAR COMPONENTE PLANO PARA BD
            // ================================
            var nuevo = new ComponenteEscandallo
            {
                IdEscandallo = idEscandallo,
                CodigoArticulo = articulo.Codigo,
                Descripcion = articulo.Descrip ?? "SIN DESCRIPCIÓN",
                Descripcion2 = articulo.Descrip2,
                Cantidad = cantidad,
                PrecioUnitario = articulo.PrecioCompra ?? 0,
                CodigoComponentePadre = padre,

                // MUY IMPORTANTE: evitar que EF intente mapear relaciones
                Hijos = null,
                Escandallo = null
            };

            // ================================
            // GUARDAR EN BD
            // ================================
            await _escandalloRepository.InsertComponenteAsync(nuevo);

            // ================================
            // GUARDAR HIJOS RECURSIVAMENTE
            // ================================
            if (comp.Hijos != null)
            {
                foreach (var hijo in comp.Hijos)
                    await GuardarComponenteRecursivo(hijo, idEscandallo, nuevo.CodigoArticulo);
            }
        }


        // ============================================================
        //   CARGAR ESCANDALLO
        // ============================================================

        public async Task CargarEscandalloAsync(string codigo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigo))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes introducir un código.");
                    return;
                }

                var esc = await _escandalloRepository.Query()
                    .FirstOrDefaultAsync(e => e.CodigoProducto == codigo);

                if (esc == null)
                {
                    MensajeError.Mostrar("ESCANDALLO", $"No existe escandallo para '{codigo}'.");
                    return;
                }

                ArticuloFinal = await _articuloRepository.GetByCodigoAsync(esc.CodigoProducto);
                OnPropertyChanged(nameof(ArticuloFinal));
                OnPropertyChanged(nameof(DescripcionFinal));
                OnPropertyChanged(nameof(Descripcion2Final));

                DescripcionArticulo = $"{ArticuloFinal.Descrip} - {ArticuloFinal.Descrip2}";

                var componentes = await _escandalloRepository.GetComponentesByEscandalloAsync(esc.IdEscandallo);

                ConstruirJerarquiaParaListar(componentes);
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ESCANDALLO", $"Error al cargar escandallo:\n{ex.Message}");
            }
        }

        private void ConstruirJerarquiaParaListar(List<ComponenteEscandallo> planos)
        {
            foreach (var comp in planos)
            {
                if (comp.Hijos == null)
                    comp.Hijos = new ObservableCollection<ComponenteEscandallo>();
            }

            var mapa = planos.ToDictionary(c => c.CodigoArticulo, c => c);

            foreach (var comp in planos)
            {
                if (!string.IsNullOrWhiteSpace(comp.CodigoComponentePadre) &&
                    mapa.TryGetValue(comp.CodigoComponentePadre, out var padre))
                {
                    padre.Hijos.Add(comp);
                }
            }

            var raices = planos.Where(c => string.IsNullOrWhiteSpace(c.CodigoComponentePadre)).ToList();

            EscandalloActual.Clear();
            foreach (var raiz in raices)
                EscandalloActual.Add(raiz);

            OnPropertyChanged(nameof(EscandalloActual));
        }

        public async Task CargarEscandallo(string codigo)
        {
            await CargarEscandalloAsync(codigo);
        }
    }
}
