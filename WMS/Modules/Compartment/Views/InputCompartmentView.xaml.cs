using System;
using System.Windows;
using System.Windows.Controls;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.Compartment
{
    /// <summary>
    /// Interaction logic for WmsInputCompartmentView.xaml
    /// </summary>
    public partial class InputCompartmentView : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CompartmentSelectedProperty = DependencyProperty.Register(
                    nameof(CompartmentSelected), typeof(CompartmentDetails), typeof(InputCompartmentView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCompartmentSelectedChanged)));

        #endregion Fields

        //private WmsCompartmentViewModel compartmentViewModel;

        #region Constructors

        public InputCompartmentView()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public CompartmentDetails CompartmentSelected
        {
            get => (CompartmentDetails)this.GetValue(CompartmentSelectedProperty);
            set => this.SetValue(CompartmentSelectedProperty, value);
        }

        #endregion Properties

        //public CompartmentDetails CompartmentSelected { get; set; }

        #region Methods

        private static void OnCompartmentSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion Methods
    }
}
