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

        public ObservableCollection<ComponenteEscandallo> Componentes { get; set; } = new();
        public ComponenteEscandallo ComponenteNuevo { get; set; }
        public ComponenteEscandallo ComponentePadreSeleccionado { get; set; }

        public ObservableCollection<Articulo> ArticulosPT { get; set; } = new();
        public ObservableCollection<Articulo> ArticulosNoPT { get; set; } = new();

        public async Task Inicializa()
        {
            var lista = await _articuloRepository.GetAllAsync();

            ArticulosPT.Clear();
            foreach (var a in lista.Where(a => a.Codigo.StartsWith("PT")))
                ArticulosPT.Add(a);

            ArticulosNoPT.Clear();
            foreach (var a in lista.Where(a => !a.Codigo.StartsWith("PT")))
                ArticulosNoPT.Add(a);
        }

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

        public void AñadirSubcomponente()
        {
            if (ComponentePadreSeleccionado == null)
            {
                MensajeError.Mostrar("ESCANDALLO", "Debes seleccionar un componente padre.");
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
                Hijos = new ObservableCollection<ComponenteEscandallo>()
            });

            OnPropertyChanged(nameof(Componentes));

            ComponenteNuevo = new ComponenteEscandallo() { Cantidad = 1 };
            OnPropertyChanged(nameof(ComponenteNuevo));
        }

        public async Task GuardarEscandallo()
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

        public async Task CargarEscandallo(string codigo)
        {
            var esc = await _escandalloRepository.Query()
                .FirstOrDefaultAsync(e => e.CodigoProducto == codigo);

            if (esc == null)
            {
                MensajeError.Mostrar("ESCANDALLO", "No existe escandallo para ese código.");
                return;
            }

            ArticuloFinal = await _articuloRepository.GetByCodigoAsync(esc.CodigoProducto);

            var componentes = await _escandalloRepository.GetComponentesByEscandalloAsync(esc.IdEscandallo);

            ConstruirJerarquia(componentes);
        }

        private void ConstruirJerarquia(List<ComponenteEscandallo> planos)
        {
            foreach (var comp in planos)
                comp.Hijos = new ObservableCollection<ComponenteEscandallo>();

            var mapa = planos.ToDictionary(c => c.CodigoArticulo, c => c);

            foreach (var comp in planos)
            {
                if (!string.IsNullOrWhiteSpace(comp.CodigoComponentePadre) &&
                    mapa.TryGetValue(comp.CodigoComponentePadre, out var padre))
                {
                    padre.Hijos.Add(comp);
                }
            }

            var raiz = planos.Where(c => string.IsNullOrWhiteSpace(c.CodigoComponentePadre)).ToList();

            Componentes.Clear();
            foreach (var r in raiz)
                Componentes.Add(r);

            OnPropertyChanged(nameof(Componentes));
        }
    }
}
