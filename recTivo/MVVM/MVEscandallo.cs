using recTivo.Frontend.Mensajes;
using recTivo.Backend.Modelos;
using recTivo.Backend.Repos;
using recTivo.MVVM.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

            _listaArticulos = new List<Articulo>();
            _articulosFiltrados = new List<Articulo>();
            _articulosTodos = new List<Articulo>();
            _articulosSinEscandallo = new List<Articulo>();
            _codigosArticulos = new List<string>();
            _articuloFinal = null;
            _articuloComponenteSeleccionado = null;
            _componenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
            _componentePadreSeleccionado = null;
            _componenteSeleccionado = null;
            _articuloSeleccionado = null;
            CodigoSeleccionado = string.Empty;
            _codigoSeleccionadoModificar = string.Empty;
        }

        public bool EsNuevoEscandallo { get; set; } = true;

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

        private List<Articulo> _articulosFiltrados;
        public List<Articulo> ArticulosFiltrados
        {
            get => _articulosFiltrados;
            set => SetProperty(ref _articulosFiltrados, value);
        }

        private List<Articulo> _articulosTodos;
        public List<Articulo> ArticulosTodos
        {
            get => _articulosTodos;
            set => SetProperty(ref _articulosTodos, value);
        }

        private List<Articulo> _articulosSinEscandallo;
        public List<Articulo> ArticulosSinEscandallo
        {
            get => _articulosSinEscandallo;
            set => SetProperty(ref _articulosSinEscandallo, value);
        }

        private List<string> _codigosArticulos;
        public List<string> CodigosArticulos
        {
            get => _codigosArticulos;
            set => SetProperty(ref _codigosArticulos, value);
        }

        public string DescripcionFinal => ArticuloFinal?.descrip ?? "";
        public string Descripcion2Final => ArticuloFinal?.descrip2 ?? "";

        private Articulo? _articuloFinal;
        public Articulo? ArticuloFinal
        {
            get => _articuloFinal;
            set
            {
                SetProperty(ref _articuloFinal, value);
                OnPropertyChanged(nameof(DescripcionFinal));
                OnPropertyChanged(nameof(Descripcion2Final));
            }
        }

        private string _descripcion = "";
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        private string _descripcion2 = "";
        public string Descripcion2
        {
            get => _descripcion2;
            set => SetProperty(ref _descripcion2, value);
        }

        private bool _articuloFinalValido = true;
        public bool ArticuloFinalValido
        {
            get => _articuloFinalValido;
            set => SetProperty(ref _articuloFinalValido, value);
        }

        public async Task<bool> ValidarArticuloFinal(Articulo? articulo)
        {
            if (articulo == null)
            {
                ArticuloFinalValido = true;
                EscandalloActual.Clear();
                return true;
            }

            var existente = await _escandalloRepository.GetByCodigoProductoAsync(articulo.Codigo);
            if (existente != null)
            {
                ArticuloFinalValido = false;
                MensajeError.Mostrar("ALTA ESCANDALLO",
                    $"El artículo '{articulo.Codigo}' ya tiene un escandallo creado.\n" +
                    $"Usa la opción MODIFICAR ESCANDALLO para editarlo.");
                ArticuloFinal = null;
                EscandalloActual.Clear();
                OnPropertyChanged(nameof(ArticuloFinal));
                OnPropertyChanged(nameof(DescripcionFinal));
                OnPropertyChanged(nameof(Descripcion2Final));
                return false;
            }

            ArticuloFinalValido = true;
            return true;
        }

        private Articulo? _articuloComponenteSeleccionado;
        public Articulo? ArticuloComponenteSeleccionado
        {
            get => _articuloComponenteSeleccionado;
            set
            {
                SetProperty(ref _articuloComponenteSeleccionado, value);

                if (value != null)
                {
                    ComponenteNuevo.CodigoArticulo = value.Codigo;
                    OnPropertyChanged(nameof(ComponenteNuevo));
                }

                OnPropertyChanged(nameof(DescripcionComponente));
                OnPropertyChanged(nameof(Descripcion2Componente));
            }
        }

        public string DescripcionComponente => ArticuloComponenteSeleccionado?.descrip ?? "";
        public string Descripcion2Componente => ArticuloComponenteSeleccionado?.descrip2 ?? "";

        private ComponenteEscandallo _componenteNuevo;
        public ComponenteEscandallo ComponenteNuevo
        {
            get => _componenteNuevo;
            set => SetProperty(ref _componenteNuevo, value);
        }

        private ComponenteEscandallo? _componentePadreSeleccionado;
        public ComponenteEscandallo? ComponentePadreSeleccionado
        {
            get => _componentePadreSeleccionado;
            set => SetProperty(ref _componentePadreSeleccionado, value);
        }

        private ComponenteEscandallo? _componenteSeleccionado;
        public ComponenteEscandallo? ComponenteSeleccionado
        {
            get => _componenteSeleccionado;
            set => SetProperty(ref _componenteSeleccionado, value);
        }

        private Articulo? _articuloSeleccionado;
        public Articulo? ArticuloSeleccionado
        {
            get => _articuloSeleccionado;
            set
            {
                SetProperty(ref _articuloSeleccionado, value);
                CodigoSeleccionado = value?.Codigo ?? string.Empty;
            }
        }

        public string CodigoSeleccionado { get; set; }

        private bool _escandalloVisible;
        public bool EscandalloVisible
        {
            get => _escandalloVisible;
            set => SetProperty(ref _escandalloVisible, value);
        }

        private string? _codigoSeleccionadoModificar;
        public string? CodigoSeleccionadoModificar
        {
            get => _codigoSeleccionadoModificar;
            set => SetProperty(ref _codigoSeleccionadoModificar, value);
        }

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

                ArticulosTodos = ListaArticulos
                    .Where(a =>
                        !a.Codigo.StartsWith("MP") &&
                        !a.Codigo.StartsWith("HE") &&
                        a.Codigo != "01" &&
                        a.Codigo != "02" &&
                        a.Codigo != "03"
                    )
                    .OrderBy(a => a.Codigo)
                    .ToList();

                ArticulosFiltrados = ListaArticulos
                    .Where(a =>
                        a.Codigo.StartsWith("PS") ||
                        a.Codigo.StartsWith("MP") ||
                        a.Codigo.StartsWith("03") ||
                        a.Codigo.StartsWith("02") ||
                        a.Codigo.StartsWith("01")
                    )
                    .OrderBy(a =>
                        a.Codigo.StartsWith("PS") ? 1 :
                        a.Codigo.StartsWith("03") ? 2 :
                        a.Codigo.StartsWith("02") ? 3 :
                        a.Codigo.StartsWith("01") ? 4 : 5
                    )
                    .ThenBy(a => a.Codigo)
                    .ToList();

                var todosEscandallos = await _escandalloRepository.GetAllAsync();
                CodigosArticulos = todosEscandallos
                    .Select(e => e.CodigoProducto)
                    .OrderBy(c => c)
                    .ToList();

                var codigosConEscandallo = new HashSet<string>(CodigosArticulos);
                ArticulosSinEscandallo = ArticulosTodos
                    //.Where(a => !codigosConEscandallo.Contains(a.Codigo)) MUESTRA TODOS LOS ARTÍCULOS PARA PODER CREAR ESCANDALLO NUEVO A CUALQUIERA
                    .ToList();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR", $"Error al inicializar: {ex.Message}");
            }
        }

        public Task LimpiarCampos()
        {
            EsNuevoEscandallo = true;

            EscandalloActual.Clear();
            ArticuloFinal = null;
            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
            ArticuloComponenteSeleccionado = null;

            OnPropertyChanged(nameof(ArticuloFinal));
            OnPropertyChanged(nameof(ComponenteNuevo));
            OnPropertyChanged(nameof(ArticuloComponenteSeleccionado));
            OnPropertyChanged(nameof(DescripcionComponente));
            OnPropertyChanged(nameof(Descripcion2Componente));
            OnPropertyChanged(nameof(EscandalloActual));

            return Task.CompletedTask;
        }

        public async Task AñadirComponente()
        {
            try
            {
                if (ArticuloFinal == null || string.IsNullOrWhiteSpace(ArticuloFinal.Codigo))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un artículo final.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(ComponenteNuevo.CodigoArticulo) || ComponenteNuevo.Cantidad <= 0)
                {
                    MensajeError.Mostrar("ESCANDALLO", "Código o cantidad inválidos.");
                    return;
                }

                if (ComponenteNuevo.CodigoArticulo == ArticuloFinal.Codigo)
                {
                    MensajeError.Mostrar("ESCANDALLO", "Un artículo no puede ser componente de sí mismo.");
                    return;
                }

                var articulo = ListaArticulos.FirstOrDefault(a => a.Codigo == ComponenteNuevo.CodigoArticulo);
                if (articulo == null)
                {
                    MensajeError.Mostrar("ESCANDALLO", $"No se encontró el artículo '{ComponenteNuevo.CodigoArticulo}'.");
                    return;
                }

                var nuevo = new ComponenteEscandallo
                {
                    CodigoArticulo = ComponenteNuevo.CodigoArticulo,
                    Cantidad = ComponenteNuevo.Cantidad,
                    Descripcion = articulo.descrip,
                    Descripcion2 = articulo.descrip2,
                    PrecioUnitario = articulo.PrecioCompra ?? 0,
                    CodigoComponentePadre = null,
                    Hijos = new ObservableCollection<ComponenteEscandallo>()
                };

                var existente = EscandalloActual
                    .FirstOrDefault(c => c.CodigoArticulo == nuevo.CodigoArticulo);

                if (existente != null)
                {
                    existente.Cantidad += nuevo.Cantidad;
                    MensajeInformacion.Mostrar("ESCANDALLO",
                        $"El componente '{nuevo.CodigoArticulo}' ya existía. Se ha sumado la cantidad.");
                }
                else
                {
                    EscandalloActual.Add(nuevo);
                }

                await CargarEscandalloDeComponente(nuevo);

                ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
                ArticuloComponenteSeleccionado = null;

                OnPropertyChanged(nameof(EscandalloActual));
                OnPropertyChanged(nameof(ComponenteNuevo));
                OnPropertyChanged(nameof(ArticuloComponenteSeleccionado));
                OnPropertyChanged(nameof(DescripcionComponente));
                OnPropertyChanged(nameof(Descripcion2Componente));
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ESCANDALLO", $"Error al añadir componente:\n{ex.Message}");
            }
        }

        private async Task CargarEscandalloDeComponente(ComponenteEscandallo componente)
        {
            try
            {
                var esc = await _escandalloRepository.GetByCodigoProductoAsync(componente.CodigoArticulo);
                if (esc == null)
                    return;

                var hijos = await _escandalloRepository.GetComponentesByEscandalloAsync(esc.IdEscandallo);
                if (hijos == null || hijos.Count == 0)
                    return;

                var hijosJerarquia = ReconstruirJerarquia(hijos);

                foreach (var hijoPlano in hijosJerarquia)
                {
                    await InsertarHijoSinDuplicar(componente, hijoPlano);
                }
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ESCANDALLO", $"Error al cargar subcomponentes:\n{ex.Message}");
            }
        }

        private async Task InsertarHijoSinDuplicar(ComponenteEscandallo padre, ComponenteEscandallo hijoPlano)
        {
            var existente = padre.Hijos?
                .FirstOrDefault(h => h.CodigoArticulo == hijoPlano.CodigoArticulo);

            if (existente != null)
            {
                existente.Cantidad += hijoPlano.Cantidad;
            }
            else
            {
                var nuevoHijo = new ComponenteEscandallo
                {
                    CodigoArticulo = hijoPlano.CodigoArticulo,
                    Cantidad = hijoPlano.Cantidad,
                    Descripcion = hijoPlano.Descripcion,
                    Descripcion2 = hijoPlano.Descripcion2,
                    PrecioUnitario = hijoPlano.PrecioUnitario,
                    CodigoComponentePadre = padre.CodigoArticulo,
                    Hijos = new ObservableCollection<ComponenteEscandallo>()
                };

                padre.Hijos?.Add(nuevoHijo);
                await CargarEscandalloDeComponente(nuevoHijo);
            }
        }

        private List<ComponenteEscandallo> ReconstruirJerarquia(List<ComponenteEscandallo> planos)
        {
            foreach (var comp in planos)
            {
                var articulo = ListaArticulos.FirstOrDefault(a => a.Codigo == comp.CodigoArticulo);
                if (articulo != null)
                {
                    comp.Descripcion = articulo.descrip;
                    comp.Descripcion2 = articulo.descrip2;
                }

                comp.Hijos ??= new ObservableCollection<ComponenteEscandallo>();
            }

            foreach (var comp in planos)
            {
                if (!string.IsNullOrWhiteSpace(comp.CodigoComponentePadre))
                {
                    var padre = planos
                        .Where(p =>
                            p.CodigoArticulo == comp.CodigoComponentePadre &&
                            p.IdEscandallo == comp.IdEscandallo &&
                            p.IdComponente != comp.IdComponente)
                        .OrderByDescending(p => p.IdComponente)
                        .FirstOrDefault();

                    if (padre != null)
                        padre.Hijos?.Add(comp);
                }
            }

            return planos
                .Where(c =>
                    string.IsNullOrWhiteSpace(c.CodigoComponentePadre) ||
                    !planos.Any(p => p.CodigoArticulo == c.CodigoComponentePadre))
                .ToList();
        }

        private async Task<bool> RecargarEscandalloRecursivo(ComponenteEscandallo componente)
        {
            bool seRecargo = false;

            var articulo = await _articuloRepository.GetByCodigoAsync(componente.CodigoArticulo);
            if (articulo != null)
            {
                componente.Descripcion = articulo.descrip;
                componente.Descripcion2 = articulo.descrip2;
            }

            var escandallo = await _escandalloRepository
                .GetByCodigoProductoAsync(componente.CodigoArticulo);

            if (escandallo != null)
            {
                var subComponentes = await _escandalloRepository
                    .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                if (subComponentes.Any())
                {
                    var hijosReconstruidos = ReconstruirJerarquia(subComponentes);

                    componente.Hijos = new ObservableCollection<ComponenteEscandallo>();

                    foreach (var hijo in hijosReconstruidos)
                    {
                        hijo.CodigoComponentePadre = componente.CodigoArticulo;

                        var artHijo = await _articuloRepository.GetByCodigoAsync(hijo.CodigoArticulo);
                        if (artHijo != null)
                        {
                            hijo.Descripcion = artHijo.descrip;
                            hijo.Descripcion2 = artHijo.descrip2;
                        }

                        componente.Hijos.Add(hijo);
                        await RecargarEscandalloRecursivo(hijo);
                    }

                    seRecargo = true;
                }
            }

            return seRecargo;
        }

        private async Task ConstruirJerarquiaParaListar(List<ComponenteEscandallo> componentes)
        {
            Debug.WriteLine($"→ Reconstruyendo jerarquía...");
            var raices = ReconstruirJerarquia(componentes);

            Debug.WriteLine($"→ Recargando escandallos de componentes...");
            foreach (var raiz in raices)
            {
                Debug.WriteLine($"   → Intentando recargar: {raiz.CodigoArticulo}");
                var recargado = await RecargarEscandalloRecursivo(raiz);
                Debug.WriteLine($"   → {raiz.CodigoArticulo} recargado: {recargado}");
            }

            Debug.WriteLine("   → Recarga completada");

            EscandalloActual.Clear();
            foreach (var raiz in raices)
                EscandalloActual.Add(raiz);

            OnPropertyChanged(nameof(EscandalloActual));
            Debug.WriteLine($"→ EscandalloActual.Count = {EscandalloActual.Count}");
        }

        public async Task GuardarEscandallo()
        {
            try
            {
                if (ArticuloFinal == null || string.IsNullOrWhiteSpace(ArticuloFinal.Codigo))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un artículo final.");
                    return;
                }

                if (EscandalloActual.Count == 0)
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes añadir al menos un componente.");
                    return;
                }

                Escandallo? existente = null;

                if (!EsNuevoEscandallo)
                {
                    existente = await _escandalloRepository
                        .GetByCodigoProductoAsync(ArticuloFinal.Codigo);

                    if (existente != null)
                    {
                        var componentesExistentes = await _escandalloRepository
                            .GetComponentesByEscandalloAsync(existente.IdEscandallo);

                        foreach (var comp in componentesExistentes)
                            await _escandalloRepository.DeleteComponenteAsync(comp.IdComponente);

                        await _escandalloRepository.DeleteByIdAsync(existente.IdEscandallo);
                    }
                }

                var nuevoEsc = new Escandallo
                {
                    CodigoProducto = ArticuloFinal.Codigo,
                    Descrip = ArticuloFinal.descrip,
                    Descrip2 = ArticuloFinal.descrip2
                };

                await _escandalloRepository.AddAsync(nuevoEsc);

                foreach (var raiz in EscandalloActual)
                    await GuardarComponenteRecursivo(raiz, nuevoEsc.IdEscandallo, null);

                MensajeInformacion.Mostrar("ESCANDALLO", "Escandallo guardado correctamente.", 1);

                if (!CodigosArticulos.Contains(ArticuloFinal.Codigo))
                {
                    CodigosArticulos = CodigosArticulos.Append(ArticuloFinal.Codigo).OrderBy(c => c).ToList();
                    var codigosConEscandallo = new HashSet<string>(CodigosArticulos);
                    ArticulosSinEscandallo = ArticulosTodos
                        //.Where(a => !codigosConEscandallo.Contains(a.Codigo)) MUESTRA TODOS LOS ARTÍCULOS PARA PODER CREAR ESCANDALLO NUEVO A CUALQUIERA
                        .ToList();
                }

                EsNuevoEscandallo = true;
                EscandalloActual.Clear();
                ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
                ArticuloComponenteSeleccionado = null;

                OnPropertyChanged(nameof(EscandalloActual));
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ESCANDALLO", $"Error al guardar: {ex.Message}");
            }
        }

        private async Task GuardarComponenteRecursivo(
            ComponenteEscandallo comp,
            int idEscandallo,
            string? codigoPadre)
        {
            var articulo = await _articuloRepository.GetByCodigoAsync(comp.CodigoArticulo);

            if (articulo == null)
            {
                articulo = new Articulo
                {
                    Codigo = comp.CodigoArticulo,
                    descrip = comp.Descripcion ?? "SIN DESCRIPCIÓN",
                    descrip2 = comp.Descripcion2,
                    PrecioCompra = comp.PrecioUnitario ?? 0
                };
            }

            var cantidad = comp.Cantidad ?? 0;
            if (cantidad <= 0)
            {
                MensajeError.Mostrar("ESCANDALLO", $"Cantidad inválida para '{comp.CodigoArticulo}'.");
                return;
            }

            var nuevo = new ComponenteEscandallo
            {
                IdEscandallo = idEscandallo,
                CodigoArticulo = articulo.Codigo,
                Descripcion = articulo.descrip ?? "SIN DESCRIPCIÓN",
                Descripcion2 = articulo.descrip2,
                Cantidad = cantidad,
                PrecioUnitario = articulo.PrecioCompra ?? 0,
                CodigoComponentePadre = codigoPadre,
                Hijos = null,
                Escandallo = null
            };

            await _escandalloRepository.InsertComponenteAsync(nuevo);

            if (comp.Hijos != null && comp.Hijos.Count > 0)
            {
                foreach (var hijo in comp.Hijos)
                    await GuardarComponenteRecursivo(hijo, idEscandallo, comp.CodigoArticulo);
            }
        }

        public async Task CargarEscandallo(string? codigo)
        {
            try
            {
                codigo = codigo?.Trim();
                if (string.IsNullOrWhiteSpace(codigo))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un código válido.");
                    return;
                }

                var escandallo = await _escandalloRepository.GetByCodigoProductoAsync(codigo);

                if (escandallo == null)
                {
                    EsNuevoEscandallo = true;
                    MensajeInformacion.Mostrar("ESCANDALLO", $"Creando escandallo nuevo para '{codigo}'.");
                    return;
                }

                EsNuevoEscandallo = false;
                EscandalloActual.Clear();

                ArticuloFinal = await _articuloRepository.GetByCodigoAsync(escandallo.CodigoProducto);

                Descripcion = ArticuloFinal?.descrip ?? "";
                Descripcion2 = ArticuloFinal?.descrip2 ?? "";
                OnPropertyChanged(nameof(ArticuloFinal));
                OnPropertyChanged(nameof(DescripcionFinal));
                OnPropertyChanged(nameof(Descripcion2Final));

                var componentes = await _escandalloRepository
                    .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                if (componentes == null || !componentes.Any())
                {
                    MensajeInformacion.Mostrar("ESCANDALLO", $"El escandallo de '{codigo}' no tiene componentes.");
                    return;
                }

                await ConstruirJerarquiaParaListar(componentes);
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR", $"Error al cargar escandallo:\n{ex.Message}");
            }
        }

        public async Task<bool> TieneEscandallo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;
            var esc = await _escandalloRepository.GetByCodigoProductoAsync(codigo.Trim());
            return esc != null;
        }

        public async Task<bool> CargarEscandalloParaModificar()
        {
            if (string.IsNullOrWhiteSpace(CodigoSeleccionadoModificar))
            {
                MensajeError.Mostrar("MODIFICAR ESCANDALLO", "Debes seleccionar un código primero.");
                return false;
            }

            await CargarEscandallo(CodigoSeleccionadoModificar);

            if (EscandalloActual.Count == 0)
                return false;

            EscandalloVisible = true;
            return true;
        }

        public bool ActualizarCantidad(ComponenteEscandallo? componente)
        {
            if (componente == null) return false;

            if (componente.Cantidad <= 0)
            {
                MensajeError.Mostrar("MODIFICAR ESCANDALLO", "La cantidad debe ser mayor que 0.");
                return false;
            }

            MensajeInformacion.Mostrar("MODIFICAR ESCANDALLO",
                $"Cantidad de '{componente.CodigoArticulo}' actualizada a {componente.Cantidad}.");
            return true;
        }

        public void QuitarComponente(ComponenteEscandallo? componente)
        {
            if (componente == null) return;

            if (EscandalloActual.Remove(componente))
                return;

            QuitarComponenteRecursivo(EscandalloActual, componente);
        }

        private bool QuitarComponenteRecursivo(
            ObservableCollection<ComponenteEscandallo> lista,
            ComponenteEscandallo objetivo)
        {
            foreach (var item in lista)
            {
                if (item.Hijos != null && item.Hijos.Remove(objetivo))
                    return true;

                if (item.Hijos != null && QuitarComponenteRecursivo(item.Hijos, objetivo))
                    return true;
            }
            return false;
        }

        public async Task GuardarYLimpiar()
        {
            await GuardarEscandallo();
            EscandalloVisible = false;
            CodigoSeleccionadoModificar = null;
            OnPropertyChanged(nameof(CodigoSeleccionadoModificar));
        }
    }
}