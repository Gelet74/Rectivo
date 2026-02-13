using di.proyecto.clase._2025.Frontend.Mensajes;
using Microsoft.EntityFrameworkCore;
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

            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
        }

        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set => SetProperty(ref _estaCargando, value);
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

        private List<Articulo> _articulosFiltrados;
        public List<Articulo> ArticulosFiltrados
        {
            get => _articulosFiltrados;
            set => SetProperty(ref _articulosFiltrados, value);
        }

        public List<string> CodigosArticulos { get; set; }

        public string DescripcionFinal => ArticuloFinal?.descrip ?? "";
        public string Descripcion2Final => ArticuloFinal?.descrip2 ?? "";
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

                ArticulosFiltrados = ListaArticulos
                    .Where(a =>
                        a.Codigo.StartsWith("PT") ||
                        a.Codigo.StartsWith("PS") ||
                        (a.Codigo.StartsWith("03") && a.Codigo.Length > 2) ||
                        (a.Codigo.StartsWith("02") && a.Codigo.Length > 2) ||
                        (a.Codigo.StartsWith("01") && a.Codigo.Length > 2)
                    )
                    .OrderBy(a =>
                        a.Codigo.StartsWith("PT") ? 1 :
                        a.Codigo.StartsWith("PS") ? 2 :
                        a.Codigo.StartsWith("03") ? 3 :
                        a.Codigo.StartsWith("02") ? 4 :
                        a.Codigo.StartsWith("01") ? 5 : 6
                    )
                    .ThenBy(a => a.Codigo)
                    .ToList();

                var todosEscandallos = await _escandalloRepository.GetAllAsync();
                CodigosArticulos = todosEscandallos
                    .Select(e => e.CodigoProducto)
                    .OrderBy(c => c)
                    .ToList();
            }
            catch (Exception ex)
            {
                MensajeError.Mostrar("ERROR", $"Error al inicializar: {ex.Message}");
            }
        }
        // ============================================================
        //   LIMPIAR CAMPOS
        // ============================================================

        public async Task LimpiarCampos()
        {
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
        }


        // ============================================================
        //   AÑADIR COMPONENTE RAÍZ
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

            var nuevoComponente = new ComponenteEscandallo
            {
                CodigoArticulo = ComponenteNuevo.CodigoArticulo,
                Cantidad = ComponenteNuevo.Cantidad,
                Descripcion = articulo?.descrip ?? "",
                Descripcion2 = articulo?.descrip2 ?? "",
                PrecioUnitario = articulo?.PrecioCompra ?? 0,
                CodigoComponentePadre = null,
                Hijos = new ObservableCollection<ComponenteEscandallo>()
            };

            await CargarEscandalloDeComponente(nuevoComponente);

            EscandalloActual.Add(nuevoComponente);

            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
            ArticuloComponenteSeleccionado = null;

            OnPropertyChanged(nameof(ComponenteNuevo));
            OnPropertyChanged(nameof(ArticuloComponenteSeleccionado));
            OnPropertyChanged(nameof(DescripcionComponente));
            OnPropertyChanged(nameof(Descripcion2Componente));
            OnPropertyChanged(nameof(EscandalloActual));
        }

        

        // ============================================================
        //   CARGAR ESCANDALLO DE UN COMPONENTE
        // ============================================================

        private async Task CargarEscandalloDeComponente(ComponenteEscandallo componente)
        {
            try
            {
                var escandallo = await _escandalloRepository
                    .GetByCodigoProductoAsync(componente.CodigoArticulo);

                if (escandallo != null)
                {
                    var subComponentes = await _escandalloRepository
                        .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                    if (subComponentes.Any())
                    {
                        var hijosReconstruidos = ReconstruirJerarquia(subComponentes);

                        componente.Hijos = new ObservableCollection<ComponenteEscandallo>(hijosReconstruidos);

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
                Debug.WriteLine($"No se pudo cargar escandallo para {componente.CodigoArticulo}: {ex.Message}");
            }
        }

        // ============================================================
        //   RECONSTRUIR JERARQUÍA
        // ============================================================

        private List<ComponenteEscandallo> ReconstruirJerarquia(List<ComponenteEscandallo> planos)
        {
            foreach (var comp in planos)
            {
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
                    {
                        padre.Hijos.Add(comp);
                    }
                }
            }

            var raices = planos
                .Where(c =>
                    string.IsNullOrWhiteSpace(c.CodigoComponentePadre) ||
                    !planos.Any(p => p.CodigoArticulo == c.CodigoComponentePadre))
                .ToList();

            return raices;
        }

        // ============================================================
        //   ACTUALIZAR PADRES RECURSIVO
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

                var existente = await _escandalloRepository
                    .GetByCodigoProductoAsync(ArticuloFinal.Codigo);

                if (existente != null)
                {
                    var componentesExistentes = await _escandalloRepository
                        .GetComponentesByEscandalloAsync(existente.IdEscandallo);

                    foreach (var comp in componentesExistentes)
                    {
                        await _escandalloRepository.DeleteComponenteAsync(comp.IdComponente);
                    }

                    await _escandalloRepository.DeleteByIdAsync(existente.IdEscandallo);
                }

                var nuevoEsc = new Escandallo
                {
                    CodigoProducto = ArticuloFinal.Codigo,
                    Descrip = ArticuloFinal.descrip,
                    Descrip2 = ArticuloFinal.descrip2
                };

                await _escandalloRepository.AddAsync(nuevoEsc);

                foreach (var raiz in EscandalloActual)
                {
                    await GuardarComponenteRecursivo(raiz, nuevoEsc.IdEscandallo, null);
                }

                MensajeInformacion.Mostrar("ESCANDALLO",
                    "Escandallo guardado correctamente.", 1);

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
                MensajeError.Mostrar("ESCANDALLO",
                    $"Cantidad inválida para '{comp.CodigoArticulo}'.");
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
                Debug.WriteLine("========== TEST BÚSQUEDA ESCANDALLO ==========");

                var todos = await _escandalloRepository.GetAllAsync();
                Debug.WriteLine($"\n→ Total de escandallos en BD: {todos.Count()}");

                foreach (var e in todos)
                {
                    Debug.WriteLine($"   ID: {e.IdEscandallo} | Código: '{e.CodigoProducto}' | Nombre: {e.Descrip}");
                }

                Debug.WriteLine("\n→ Buscando 'PS3510BB'...");
                var escPS3510BB = await _escandalloRepository.GetByCodigoProductoAsync("PS3510BB");

                if (escPS3510BB != null)
                {
                    Debug.WriteLine($"   ✓ ENCONTRADO: ID={escPS3510BB.IdEscandallo}");

                    var componentes = await _escandalloRepository.GetComponentesByEscandalloAsync(escPS3510BB.IdEscandallo);
                    Debug.WriteLine($"   ✓ Componentes: {componentes.Count}");

                    foreach (var comp in componentes)
                    {
                        Debug.WriteLine($"      - {comp.CodigoArticulo} | Cant: {comp.Cantidad} | Padre: {comp.CodigoComponentePadre ?? "NULL"}");
                    }
                }
                else
                {
                    Debug.WriteLine("   ✗ NO ENCONTRADO");
                }

                Debug.WriteLine("\n→ Probando variaciones...");

                var variaciones = new[] { "PS3510BB", "ps3510bb", "PS3510BB ", " PS3510BB" };

                foreach (var variacion in variaciones)
                {
                    var resultado = await _escandalloRepository.GetByCodigoProductoAsync(variacion);
                    Debug.WriteLine($"   '{variacion}' (len={variacion.Length}): {(resultado != null ? "✓ ENCONTRADO" : "✗ NO")}");
                }

                Debug.WriteLine("==============================================\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR en test: {ex.Message}");
            }
        }

        // ============================================================
        //   CARGAR ESCANDALLO CON RECARGA AUTOMÁTICA
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

                Debug.WriteLine($"→ Buscando escandallo para código: '{codigo}'");

                EscandalloActual.Clear();

                var escandallo = await _escandalloRepository.GetByCodigoProductoAsync(codigo);

                if (escandallo == null)
                {
                    Debug.WriteLine($"→ NO SE ENCONTRÓ escandallo para '{codigo}'");
                    MensajeInformacion.Mostrar("ESCANDALLO",
                        $"No existe escandallo para el artículo '{codigo}'.");
                    return;
                }

                Debug.WriteLine($"→ Escandallo encontrado: ID={escandallo.IdEscandallo}, Codigo={escandallo.CodigoProducto}");

                ArticuloFinal = await _articuloRepository.GetByCodigoAsync(escandallo.CodigoProducto);
                OnPropertyChanged(nameof(ArticuloFinal));
                OnPropertyChanged(nameof(DescripcionFinal));
                OnPropertyChanged(nameof(Descripcion2Final));

                DescripcionArticulo = $"{ArticuloFinal.descrip} - {ArticuloFinal.descrip2}";

                var componentes = await _escandalloRepository
                    .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                Debug.WriteLine($"→ Componentes encontrados: {componentes?.Count ?? 0}");

                if (componentes == null || !componentes.Any())
                {
                    MensajeInformacion.Mostrar("ESCANDALLO",
                        $"El escandallo de '{codigo}' no tiene componentes.");
                    return;
                }

                await ConstruirJerarquiaParaListar(componentes);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"→ ERROR: {ex.Message}");
                Debug.WriteLine($"→ StackTrace: {ex.StackTrace}");

                MensajeError.Mostrar("ERROR",
                    $"Error al cargar escandallo:\n{ex.Message}");
            }
        }

        // ============================================================
        //   CONSTRUIR JERARQUÍA PARA LISTAR (TREEVIEW)
        // ============================================================

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
            {
                EscandalloActual.Add(raiz);
            }

            OnPropertyChanged(nameof(EscandalloActual));

            Debug.WriteLine($"→ EscandalloActual.Count = {EscandalloActual.Count}");
        }

        
        // ============================================================
        //   RECARGAR ESCANDALLO RECURSIVO
        // ============================================================

        private async Task<bool> RecargarEscandalloRecursivo(ComponenteEscandallo componente)
        {
            bool seRecargo = false;

            var escandallo = await _escandalloRepository
                .GetByCodigoProductoAsync(componente.CodigoArticulo);

            if (escandallo != null)
            {
                var subComponentes = await _escandalloRepository
                    .GetComponentesByEscandalloAsync(escandallo.IdEscandallo);

                if (subComponentes.Any())
                {
                    var hijosReconstruidos = ReconstruirJerarquia(subComponentes);

                    componente.Hijos = new ObservableCollection<ComponenteEscandallo>(hijosReconstruidos);

                    foreach (var hijo in componente.Hijos)
                    {
                        hijo.CodigoComponentePadre = componente.CodigoArticulo;
                        await RecargarEscandalloRecursivo(hijo);
                    }

                    seRecargo = true;
                }
            }

            return seRecargo;
        }
    }
}
