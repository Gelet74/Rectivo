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

        // ============================================================
        //   PROPIEDADES NUEVAS PARA COMPONENTE SELECCIONADO
        // ============================================================

        private Articulo _articuloComponenteSeleccionado;
        public Articulo ArticuloComponenteSeleccionado
        {
            get => _articuloComponenteSeleccionado;
            set
            {
                SetProperty(ref _articuloComponenteSeleccionado, value);

                // Actualizar el código en ComponenteNuevo
                if (value != null)
                {
                    ComponenteNuevo.CodigoArticulo = value.Codigo;
                    OnPropertyChanged(nameof(ComponenteNuevo));
                }

                // Actualizar las descripciones
                OnPropertyChanged(nameof(DescripcionComponente));
                OnPropertyChanged(nameof(Descripcion2Componente));
            }
        }

        public string DescripcionComponente => ArticuloComponenteSeleccionado?.Descrip ?? "";
        public string Descripcion2Componente => ArticuloComponenteSeleccionado?.Descrip2 ?? "";

        // ============================================================
        //   PROPIEDADES EXISTENTES
        // ============================================================

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
        //   AÑADIR COMPONENTE RAÍZ CON CARGA AUTOMÁTICA DE ESCANDALLO
        // ============================================================

        public async Task AñadirComponente()
        {
            if (ArticuloFinal == null || string.IsNullOrWhiteSpace(ArticuloFinal.Codigo))
            {
                MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un artículo válido.");
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

            var nuevoComponente = new ComponenteEscandallo
            {
                CodigoArticulo = ComponenteNuevo.CodigoArticulo,
                Cantidad = ComponenteNuevo.Cantidad,
                Descripcion = articulo?.Descrip ?? "",
                Descripcion2 = articulo?.Descrip2 ?? "",
                PrecioUnitario = articulo?.PrecioCompra ?? 0,
                CodigoComponentePadre = null,
                Hijos = new ObservableCollection<ComponenteEscandallo>()
            };

            // CARGAR ESCANDALLO DEL COMPONENTE SI EXISTE
            await CargarEscandalloDeComponente(nuevoComponente);

            EscandalloActual.Add(nuevoComponente);

            // Limpiar selección
            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
            ArticuloComponenteSeleccionado = null;

            OnPropertyChanged(nameof(ComponenteNuevo));
            OnPropertyChanged(nameof(ArticuloComponenteSeleccionado));
            OnPropertyChanged(nameof(DescripcionComponente));
            OnPropertyChanged(nameof(Descripcion2Componente));
            OnPropertyChanged(nameof(EscandalloActual));
        }

        // ============================================================
        //   AÑADIR SUBCOMPONENTE CON CARGA AUTOMÁTICA
        // ============================================================

        public async Task AñadirSubcomponente()
        {
            if (ComponentePadreSeleccionado == null)
            {
                MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un componente padre en el árbol.");
                return;
            }

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

            if (ComponentePadreSeleccionado.Hijos == null)
                ComponentePadreSeleccionado.Hijos = new ObservableCollection<ComponenteEscandallo>();

            var nuevoHijo = new ComponenteEscandallo
            {
                CodigoArticulo = ComponenteNuevo.CodigoArticulo,
                Cantidad = ComponenteNuevo.Cantidad,
                Descripcion = articulo?.Descrip ?? "",
                Descripcion2 = articulo?.Descrip2 ?? "",
                PrecioUnitario = articulo?.PrecioCompra ?? 0,
                CodigoComponentePadre = ComponentePadreSeleccionado.CodigoArticulo,
                Hijos = new ObservableCollection<ComponenteEscandallo>()
            };

            // CARGAR ESCANDALLO DEL COMPONENTE SI EXISTE
            await CargarEscandalloDeComponente(nuevoHijo);

            ComponentePadreSeleccionado.Hijos.Add(nuevoHijo);

            OnPropertyChanged(nameof(EscandalloActual));

            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
            ArticuloComponenteSeleccionado = null;

            OnPropertyChanged(nameof(ComponenteNuevo));
            OnPropertyChanged(nameof(ArticuloComponenteSeleccionado));
            OnPropertyChanged(nameof(DescripcionComponente));
            OnPropertyChanged(nameof(Descripcion2Componente));
        }

        // ============================================================
        //   CARGAR ESCANDALLO DE UN COMPONENTE (REUTILIZACIÓN)
        // ============================================================

        private async Task CargarEscandalloDeComponente(ComponenteEscandallo componente)
        {
            try
            {
                // Buscar si existe un escandallo para este componente
                var escandallo = await _escandalloRepository
                    .GetByCodigoProductoAsync(componente.CodigoArticulo);

                if (escandallo != null)
                {
                    // Obtener todos los componentes del escandallo
                    var subComponentes = await _escandalloRepository
                        .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                    if (subComponentes.Any())
                    {
                        // Reconstruir la jerarquía de hijos
                        var hijosReconstruidos = ReconstruirJerarquia(subComponentes);

                        // Asignar los hijos al componente
                        componente.Hijos = new ObservableCollection<ComponenteEscandallo>(hijosReconstruidos);

                        // Actualizar el padre de todos los hijos
                        foreach (var hijo in componente.Hijos)
                        {
                            hijo.CodigoComponentePadre = componente.CodigoArticulo;
                            ActualizarPadresRecursivo(hijo, componente.CodigoArticulo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Si falla la carga, simplemente dejamos el componente sin hijos
                // No mostramos error porque es opcional
                System.Diagnostics.Debug.WriteLine($"No se pudo cargar escandallo para {componente.CodigoArticulo}: {ex.Message}");
            }
        }

        // ============================================================
        //   RECONSTRUIR JERARQUÍA DESDE LISTA PLANA
        // ============================================================

        private List<ComponenteEscandallo> ReconstruirJerarquia(List<ComponenteEscandallo> planos)
        {
            // Inicializar Hijos para todos
            foreach (var comp in planos)
            {
                if (comp.Hijos == null)
                    comp.Hijos = new ObservableCollection<ComponenteEscandallo>();
            }

            // Crear mapa por código
            var mapa = new Dictionary<string, ComponenteEscandallo>();
            foreach (var comp in planos)
            {
                if (!mapa.ContainsKey(comp.CodigoArticulo))
                    mapa[comp.CodigoArticulo] = comp;
            }

            // Construir jerarquía
            foreach (var comp in planos)
            {
                if (!string.IsNullOrWhiteSpace(comp.CodigoComponentePadre))
                {
                    if (mapa.TryGetValue(comp.CodigoComponentePadre, out var padre))
                    {
                        if (padre.Hijos == null)
                            padre.Hijos = new ObservableCollection<ComponenteEscandallo>();

                        padre.Hijos.Add(comp);
                    }
                }
            }

            // Retornar solo las raíces
            return planos.Where(c => string.IsNullOrWhiteSpace(c.CodigoComponentePadre)).ToList();
        }

        // ============================================================
        //   ACTUALIZAR PADRES RECURSIVAMENTE
        // ============================================================

        private void ActualizarPadresRecursivo(ComponenteEscandallo componente, string nuevoPadre)
        {
            componente.CodigoComponentePadre = nuevoPadre;

            if (componente.Hijos != null)
            {
                foreach (var hijo in componente.Hijos)
                {
                    ActualizarPadresRecursivo(hijo, componente.CodigoArticulo);
                }
            }
        }

        // ============================================================
        //   GUARDAR ESCANDALLO
        // ============================================================

        public async Task GuardarEscandallo()
        {
            try
            {
                // Validar artículo final
                if (ArticuloFinal == null || string.IsNullOrWhiteSpace(ArticuloFinal.Codigo))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un artículo final.");
                    return;
                }

                // Validar que haya componentes
                if (EscandalloActual.Count == 0)
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes añadir al menos un componente.");
                    return;
                }

                // Eliminar escandallo existente
                var existente = await _escandalloRepository
                    .GetByCodigoProductoAsync(ArticuloFinal.Codigo);

                if (existente != null)
                {
                    // Primero eliminar todos los componentes
                    var componentesExistentes = await _escandalloRepository
                        .GetComponentesByEscandalloAsync(existente.IdEscandallo);

                    foreach (var comp in componentesExistentes)
                    {
                        await _escandalloRepository.DeleteComponenteAsync(comp.IdComponente);
                    }

                    await _escandalloRepository.DeleteByIdAsync(existente.IdEscandallo);
                }

                // Crear nuevo escandallo
                var nuevoEsc = new Escandallo
                {
                    CodigoProducto = ArticuloFinal.Codigo,
                    NombreProducto = ArticuloFinal.Descrip,
                    Descripcion2 = ArticuloFinal.Descrip2
                };

                await _escandalloRepository.AddAsync(nuevoEsc);

                // GUARDAR TODA LA JERARQUÍA
                foreach (var raiz in EscandalloActual)
                {
                    await GuardarComponenteRecursivo(raiz, nuevoEsc.IdEscandallo, null);
                }

                MensajeInformacion.Mostrar("ESCANDALLO",
                    "Escandallo guardado correctamente.", 1);

                // Limpiar
                EscandalloActual.Clear();
                ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
                ArticuloComponenteSeleccionado = null;

                OnPropertyChanged(nameof(ComponenteNuevo));
                OnPropertyChanged(nameof(ArticuloComponenteSeleccionado));
                OnPropertyChanged(nameof(DescripcionComponente));
                OnPropertyChanged(nameof(Descripcion2Componente));
                OnPropertyChanged(nameof(EscandalloActual));
            }
            catch (Exception ex)
            {
                var detalle = ex.InnerException?.Message ?? ex.Message;
                MensajeError.Mostrar("ESCANDALLO", $"Error al guardar: {detalle}");
            }
        }

        // ============================================================
        //   GUARDADO RECURSIVO
        // ============================================================

        private async Task GuardarComponenteRecursivo(
            ComponenteEscandallo comp,
            int idEscandallo,
            string codigoPadre)
        {
            // Validar artículo
            var articulo = await _articuloRepository.GetByCodigoAsync(comp.CodigoArticulo);

            if (articulo == null)
            {
                articulo = new Articulo
                {
                    Codigo = comp.CodigoArticulo,
                    Descrip = comp.Descripcion ?? "SIN DESCRIPCIÓN",
                    Descrip2 = comp.Descripcion2,
                    PrecioCompra = comp.PrecioUnitario ?? 0
                };
            }

            // Validar cantidad
            var cantidad = comp.Cantidad ?? 0;
            if (cantidad <= 0)
            {
                MensajeError.Mostrar("ESCANDALLO",
                    $"Cantidad inválida para '{comp.CodigoArticulo}'.");
                return;
            }

            // Crear componente para BD
            var nuevo = new ComponenteEscandallo
            {
                IdEscandallo = idEscandallo,
                CodigoArticulo = articulo.Codigo,
                Descripcion = articulo.Descrip ?? "SIN DESCRIPCIÓN",
                Descripcion2 = articulo.Descrip2,
                Cantidad = cantidad,
                PrecioUnitario = articulo.PrecioCompra ?? 0,
                CodigoComponentePadre = codigoPadre,

                // Evitar que EF mapee relaciones
                Hijos = null,
                Escandallo = null
            };

            // Guardar en BD
            await _escandalloRepository.InsertComponenteAsync(nuevo);

            // Guardar hijos recursivamente
            if (comp.Hijos != null && comp.Hijos.Count > 0)
            {
                foreach (var hijo in comp.Hijos)
                {
                    await GuardarComponenteRecursivo(hijo, idEscandallo, comp.CodigoArticulo);
                }
            }
        }

        // ============================================================
        //   MÉTODO TEMPORAL DE DEBUG
        // ============================================================

        public async Task TestBuscarEscandallo()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("========== TEST BÚSQUEDA ESCANDALLO ==========");

                // 1. Ver TODOS los escandallos que existen
                var todos = await _escandalloRepository.GetAllAsync();
                System.Diagnostics.Debug.WriteLine($"\n→ Total de escandallos en BD: {todos.Count()}");

                foreach (var e in todos)
                {
                    System.Diagnostics.Debug.WriteLine($"   ID: {e.IdEscandallo} | Código: '{e.CodigoProducto}' | Nombre: {e.NombreProducto}");
                }

                // 2. Buscar específicamente PS3510BB
                System.Diagnostics.Debug.WriteLine("\n→ Buscando 'PS3510BB'...");
                var escPS3510BB = await _escandalloRepository.GetByCodigoProductoAsync("PS3510BB");

                if (escPS3510BB != null)
                {
                    System.Diagnostics.Debug.WriteLine($"   ✓ ENCONTRADO: ID={escPS3510BB.IdEscandallo}");

                    var componentes = await _escandalloRepository.GetComponentesByEscandalloAsync(escPS3510BB.IdEscandallo);
                    System.Diagnostics.Debug.WriteLine($"   ✓ Componentes: {componentes.Count}");

                    foreach (var comp in componentes)
                    {
                        System.Diagnostics.Debug.WriteLine($"      - {comp.CodigoArticulo} | Cant: {comp.Cantidad} | Padre: {comp.CodigoComponentePadre ?? "NULL"}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("   ✗ NO ENCONTRADO");
                }

                // 3. Buscar con variaciones
                System.Diagnostics.Debug.WriteLine("\n→ Probando variaciones...");

                var variaciones = new[] { "PS3510BB", "ps3510bb", "PS3510BB ", " PS3510BB" };

                foreach (var variacion in variaciones)
                {
                    var resultado = await _escandalloRepository.GetByCodigoProductoAsync(variacion);
                    System.Diagnostics.Debug.WriteLine($"   '{variacion}' (len={variacion.Length}): {(resultado != null ? "✓ ENCONTRADO" : "✗ NO")}");
                }

                System.Diagnostics.Debug.WriteLine("==============================================\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR en test: {ex.Message}");
            }
        }

        // ============================================================
        //   CARGAR ESCANDALLO PARA LISTAR
        // ============================================================

        public async Task CargarEscandallo(string codigo)
        {
            try
            {
                codigo = codigo?.Trim();
                if (string.IsNullOrWhiteSpace(codigo))
                {
                    MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un código válido.");
                    return;
                }

                EscandalloActual.Clear();

                System.Diagnostics.Debug.WriteLine($"→ Buscando escandallo para código: '{codigo}'");

                EscandalloActual.Clear();

                // Buscar el escandallo por código de producto
                var escandallo = await _escandalloRepository.GetByCodigoProductoAsync(codigo);

                if (escandallo == null)
                {
                    System.Diagnostics.Debug.WriteLine($"→ NO SE ENCONTRÓ escandallo para '{codigo}'");

                    // ✅ DEBUG: Ver todos los escandallos que existen
                    var todos = await _escandalloRepository.GetAllAsync();
                    System.Diagnostics.Debug.WriteLine($"→ Escandallos en BD:");
                    foreach (var e in todos)
                    {
                        System.Diagnostics.Debug.WriteLine($"   - '{e.CodigoProducto}' (ID: {e.IdEscandallo})");
                    }

                    MensajeInformacion.Mostrar("ESCANDALLO",
                        $"No existe escandallo para el artículo '{codigo}'.");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"→ Escandallo encontrado: ID={escandallo.IdEscandallo}, Codigo={escandallo.CodigoProducto}");

                // Obtener todos los componentes del escandallo
                var componentes = await _escandalloRepository
                    .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                System.Diagnostics.Debug.WriteLine($"→ Componentes encontrados: {componentes?.Count ?? 0}");

                if (componentes == null || !componentes.Any())
                {
                    MensajeInformacion.Mostrar("ESCANDALLO",
                        $"El escandallo de '{codigo}' no tiene componentes.");
                    return;
                }

                // Construir la jerarquía para el TreeView
                ConstruirJerarquiaParaListar(componentes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"→ ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"→ StackTrace: {ex.StackTrace}");

                MensajeError.Mostrar("ERROR",
                    $"Error al cargar escandallo:\n{ex.Message}");
            }
        }

        // ============================================================
        //   RECARGAR ESCANDALLOS SILENCIOSO (SIN MENSAJES)
        // ============================================================

        private async Task RecargarEscandallosDeComponentesSilencioso()
        {
            try
            {
                if (EscandalloActual.Count == 0)
                    return;

                foreach (var componente in EscandalloActual)
                {
                    await RecargarEscandalloRecursivo(componente);
                }

                OnPropertyChanged(nameof(EscandalloActual));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al recargar escandallos: {ex.Message}");
            }
        }

        // ============================================================
        //   RECARGAR ESCANDALLO RECURSIVO
        // ============================================================

        private async Task<bool> RecargarEscandalloRecursivo(ComponenteEscandallo componente)
        {
            bool seRecargo = false;

            // Buscar si existe un escandallo para este componente
            var escandallo = await _escandalloRepository
                .GetByCodigoProductoAsync(componente.CodigoArticulo);

            if (escandallo != null)
            {
                // Obtener todos los componentes del escandallo
                var subComponentes = await _escandalloRepository
                    .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                if (subComponentes.Any())
                {
                    // Reconstruir la jerarquía de hijos
                    var hijosReconstruidos = ReconstruirJerarquia(subComponentes);

                    // Reemplazar los hijos
                    componente.Hijos = new ObservableCollection<ComponenteEscandallo>(hijosReconstruidos);

                    // Actualizar el padre de todos los hijos
                    foreach (var hijo in componente.Hijos)
                    {
                        hijo.CodigoComponentePadre = componente.CodigoArticulo;
                        ActualizarPadresRecursivo(hijo, componente.CodigoArticulo);
                    }

                    seRecargo = true;
                }
            }

            // Intentar recargar los hijos también
            if (componente.Hijos != null)
            {
                foreach (var hijo in componente.Hijos)
                {
                    if (await RecargarEscandalloRecursivo(hijo))
                        seRecargo = true;
                }
            }

            return seRecargo;
        }

        // ============================================================
        //   CONSTRUIR JERARQUÍA DESDE BD
        // ============================================================

        private void ConstruirJerarquiaParaListar(List<ComponenteEscandallo> planos)
        {
            // 1. Inicializar Hijos para todos los componentes
            foreach (var comp in planos)
            {
                if (comp.Hijos == null)
                    comp.Hijos = new ObservableCollection<ComponenteEscandallo>();
            }

            // 2. Crear diccionario por código de artículo
            var mapa = planos.ToDictionary(c => c.CodigoArticulo, c => c);

            // 3. Construir jerarquía enlazando padres e hijos
            foreach (var comp in planos)
            {
                if (!string.IsNullOrWhiteSpace(comp.CodigoComponentePadre))
                {
                    if (mapa.TryGetValue(comp.CodigoComponentePadre, out var padre))
                    {
                        if (padre.Hijos == null)
                            padre.Hijos = new ObservableCollection<ComponenteEscandallo>();

                        padre.Hijos.Add(comp);
                    }
                }
            }

            // 4. Obtener solo las raíces (componentes sin padre)
            var raices = planos.Where(c => string.IsNullOrWhiteSpace(c.CodigoComponentePadre)).ToList();

            // 5. Actualizar TreeView con las raíces (que ya tienen sus hijos enlazados)
            EscandalloActual.Clear();
            foreach (var raiz in raices)
                EscandalloActual.Add(raiz);

            OnPropertyChanged(nameof(EscandalloActual));
        }
    }
}