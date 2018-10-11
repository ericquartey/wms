using System;
using System.Windows;
using System.Windows.Controls;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.Compartment
{
    /// <summary>
    /// Interaction logic for WmsInputCompartmentView.xaml
    /// </summary>
    public partial class InputCompartmentView : UserControl
    {
        //public static readonly DependencyProperty LoadingUnitProperty = DependencyProperty.Register(
        //            nameof(LoadingUnit), typeof(LoadingUnitDetails), typeof(InputCompartmentView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLoadingUnitChanged)));

        //public LoadingUnitDetails LoadingUnit
        //{
        //    get => (LoadingUnitDetails)this.GetValue(LoadingUnitProperty);
        //    set => this.SetValue(LoadingUnitProperty, value);
        //}

        #region Constructors

        public InputCompartmentView()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        //private static void OnLoadingUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is InputCompartmentView inputCompartmentView && inputCompartmentView.DataContext is InputCompartmentViewModel viewModel)
        //    {
        //        viewModel.UpdateTray((LoadingUnitDetails)e.NewValue);
        //    }
        //}
    }
}
