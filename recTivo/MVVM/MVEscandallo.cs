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
        //   PROPIEDADES BÁSICAS
        // ============================================================
        private Escandallo _escandallo;
        public Escandallo Escandallo
        {
            get => _escandallo;
            set => SetProperty(ref _escandallo, value);
        }

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

        public string DescripcionFinal => ArticuloFinal?.Descrip ?? "";
        public string Descripcion2Final => ArticuloFinal?.Descrip2 ?? "";

        // ============================================================
        //   COMPONENTES Y JERARQUÍA
        // ============================================================
        public ObservableCollection<ComponenteEscandallo> Componentes { get; set; } = new();

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

        // ============================================================
        //   LISTAS DE ARTÍCULOS
        // ============================================================
        public ObservableCollection<Articulo> ArticulosPT { get; set; } = new();
        public ObservableCollection<Articulo> ArticulosNoPT { get; set; } = new();

        private List<Articulo> _listaArticulos;
        public List<Articulo> ListaArticulos
        {
            get => _listaArticulos;
            set => SetProperty(ref _listaArticulos, value);
        }

        // ============================================================
        //   PROPIEDADES PARA COMBO BOX Y SELECCIÓN
        // ============================================================
        private string _codigoSeleccionado;
        public string CodigoSeleccionado
        {
            get => _codigoSeleccionado;
            set => SetProperty(ref _codigoSeleccionado, value);
        }

        private List<string> _codigosArticulos;
        public List<string> CodigosArticulos
        {
            get => _codigosArticulos;
            set => SetProperty(ref _codigosArticulos, value);
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

        // ============================================================
        //   ESCANDALLO ACTUAL (PARA LISTAR)
        // ============================================================
        private ObservableCollection<ComponenteEscandallo> _escandalloActual = new();
        public ObservableCollection<ComponenteEscandallo> EscandalloActual
        {
            get => _escandalloActual;
            set => SetProperty(ref _escandalloActual, value);
        }

        private ComponenteEscandallo _componenteSeleccionado;
        public ComponenteEscandallo ComponenteSeleccionado
        {
            get => _componenteSeleccionado;
            set => SetProperty(ref _componenteSeleccionado, value);
        }

        private string _descripcionArticulo;
        public string DescripcionArticulo
        {
            get => _descripcionArticulo;
            set => SetProperty(ref _descripcionArticulo, value);
        }

        // ============================================================
        //   INICIALIZACIÓN
        // ============================================================
        public async Task Inicializa()
        {
            try
            {
                var lista = await _articuloRepository.GetAllAsync();

                ArticulosPT.Clear();
                foreach (var a in lista.Where(a => a.Codigo.StartsWith("PT")))
                    ArticulosPT.Add(a);

                ArticulosNoPT.Clear();
                foreach (var a in lista
                    .Where(a => !a.Codigo.StartsWith("PT"))
                    .OrderBy(a => a.Codigo))
                {
                    ArticulosNoPT.Add(a);
                }


                ListaArticulos = lista.ToList();

                // Cargar códigos para ComboBox
                CodigosArticulos = await _articuloRepository
                    .Query(true)
                    .Select(a => a.Codigo)
                    .ToListAsync();
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
                MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un artículo PT como raíz.");
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
                if (ArticuloFinal == null || string.IsNullOrWhiteSpace(ArticuloFinal.Codigo))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un artículo final.");
                    return;
                }

                var articuloFinal = await _articuloRepository.GetByCodigoAsync(ArticuloFinal.Codigo);
                if (articuloFinal == null)
                {
                    MensajeError.Mostrar("ESCANDALLO", "El artículo final no existe.");
                    return;
                }

                var nuevoEsc = new Escandallo
                {
                    CodigoProducto = articuloFinal.Codigo,
                    NombreProducto = articuloFinal.Descrip,
                    Descripcion2 = articuloFinal.Descrip2
                };

                await _escandalloRepository.AddAsync(nuevoEsc);

                foreach (var comp in Componentes)
                    await GuardarComponenteRecursivo(comp, nuevoEsc.IdEscandallo, null);

                MensajeInformacion.Mostrar("ESCANDALLO", "Escandallo guardado correctamente.", 1);

                Componentes.Clear();
                ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
                ArticuloFinal = null;

                OnPropertyChanged(nameof(Componentes));
                OnPropertyChanged(nameof(ComponenteNuevo));
                OnPropertyChanged(nameof(ArticuloFinal));
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ESCANDALLO", $"Error al guardar: {ex.Message}");
            }
        }

        private async Task GuardarComponenteRecursivo(ComponenteEscandallo comp, int idEscandallo, string padre)
        {
            var articulo = await _articuloRepository.GetByCodigoAsync(comp.CodigoArticulo);
            if (articulo == null)
                return;

            var nuevo = new ComponenteEscandallo
            {
                IdEscandallo = idEscandallo,
                CodigoArticulo = articulo.Codigo,
                Descripcion = articulo.Descrip,
                Descripcion2 = articulo.Descrip2,
                Cantidad = comp.Cantidad,
                PrecioUnitario = articulo.PrecioCompra ?? 0,
                CodigoComponentePadre = padre
            };

            await _escandalloRepository.InsertComponenteAsync(nuevo);

            foreach (var hijo in comp.Hijos)
                await GuardarComponenteRecursivo(hijo, idEscandallo, nuevo.CodigoArticulo);
        }

        // ============================================================
        //   CARGAR ESCANDALLO (PARA LISTAR)
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

                var esc = await _escandalloRepository
                    .Query()
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

                var componentes = await _escandalloRepository
                    .GetComponentesByEscandalloAsync(esc.IdEscandallo);

                ConstruirJerarquiaParaListar(componentes);
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ESCANDALLO", $"Error al cargar escandallo:\n{ex.Message}");
            }
        }

        private void ConstruirJerarquiaParaListar(List<ComponenteEscandallo> planos)
        {
            // Inicializar colección de hijos para TODOS los componentes
            foreach (var comp in planos)
            {
                if (comp.Hijos == null)
                    comp.Hijos = new ObservableCollection<ComponenteEscandallo>();
            }

            // Crear diccionario para búsqueda rápida
            var mapa = new Dictionary<string, ComponenteEscandallo>();
            foreach (var comp in planos)
            {
                if (!mapa.ContainsKey(comp.CodigoArticulo))
                    mapa[comp.CodigoArticulo] = comp;
            }

            // Construir jerarquía: agregar cada componente a su padre
            foreach (var comp in planos)
            {
                if (!string.IsNullOrWhiteSpace(comp.CodigoComponentePadre))
                {
                    if (mapa.TryGetValue(comp.CodigoComponentePadre, out var padre))
                    {
                        padre.Hijos.Add(comp);
                    }
                }
            }

            // Obtener solo los nodos raíz (sin padre)
            var raices = planos.Where(c => string.IsNullOrWhiteSpace(c.CodigoComponentePadre)).ToList();

            // Actualizar la colección del TreeView
            EscandalloActual.Clear();
            foreach (var raiz in raices)
            {
                EscandalloActual.Add(raiz);
            }

            OnPropertyChanged(nameof(EscandalloActual));
        }

        // Alias del método para compatibilidad
        public async Task CargarEscandallo(string codigo)
        {
            await CargarEscandalloAsync(codigo);
        }

        private void ConstruirJerarquia(List<ComponenteEscandallo> planos)
        {
            ConstruirJerarquiaParaListar(planos);
        }
    }
}