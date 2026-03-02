using recTivo.MVVM;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace recTivo.Frontend.UC
{
    public partial class UCCerrarFase : UserControl
    {
        private readonly MVOrden _vm;
        public event Action? SolicitarCierre;

        public UCCerrarFase(MVOrden vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;

            Loaded += async (_, _) => await _vm.InicializarListadoAsync();

            KeyDown += (_, e) => { if (e.Key == Key.Escape) SolicitarCierre?.Invoke(); };
        }

        private async void BtnCerrarFase_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current is not App app || app.EmpleadoActual == null) return;
            await _vm.CerrarFaseActivaAsync(app.EmpleadoActual);
        }

        private async void BtnRecargar_Click(object sender, RoutedEventArgs e)
        {
            await _vm.CargarOrdenesAsync();
        }
    }
}
