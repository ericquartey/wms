using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    private static void OnLoadingUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WmsTrayControl wmsTrayControl)
        {
            if (wmsTrayControl.DataContext is WmsTrayControlViewModel viewModel)
            {
                viewModel.UpdateTray((LoadingUnitDetails)e.NewValue);
            }
        }
        var dataContext = ((WmsTrayControl)d).DataContext;
        if (dataContext != null)
        {
            var viewModel = dataContext as WmsTrayControlViewModel;
            if (viewModel != null)
            {
                viewModel.UpdateTray((LoadingUnitDetails)e.NewValue);
            }
        }
    }

    /// <summary>
    /// Interaction logic for WmsHistoryTrayControl.xaml
    /// </summary>
    public partial class WmsHistoryTrayControl : ContentControl
    {
        #region Fields

        public static readonly DependencyProperty LoadingUnitProperty = DependencyProperty.Register(
                    nameof(LoadingUnit), typeof(WmsHistoryTrayControl), typeof(WmsHistoryTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLoadingUnitChanged)));

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
                    nameof(ReadOnly), typeof(bool), typeof(WmsHistoryTrayControl), new PropertyMetadata(false));

        #endregion Fields

        #region Constructors

        public WmsHistoryTrayControl()
        {
            this.InitializeComponent();
            this.DataContext = new WmsHistoryTrayControlViewModel();
        }

        #endregion Constructors

        #region Properties

        public LoadingUnitDetails LoadingUnit
        {
            get => (LoadingUnitDetails)this.GetValue(LoadingUnitProperty);
            set => this.SetValue(LoadingUnitProperty, value);
        }

        public bool ReadOnly
        {
            get => (bool)this.GetValue(ReadOnlyProperty);
            set => this.SetValue(ReadOnlyProperty, value);
        }

        #endregion Properties

        #region Methods

        private void CreateCompartments(List<WmsCompartment> compartments)
        {
            foreach (var comp in compartments)
            {
                WmsCompartmentControl compartment = new WmsCompartmentControl();
                compartment.updaCompartment = comp;
            }
        }

        #endregion Methods
    }
}
