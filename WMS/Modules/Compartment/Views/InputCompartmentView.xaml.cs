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

        #region Methods

        private static void OnCompartmentSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private void DisenableAllInput()
        {
            this.WidthText.IsEnabled = false;
            this.HeightText.IsEnabled = false;
            this.PositionXText.IsEnabled = false;
            this.PositionYText.IsEnabled = false;
            this.ArticleText.IsEnabled = false;
            this.QuantityText.IsEnabled = false;
            this.CapacityText.IsEnabled = false;
            this.CreateCompartment.IsEnabled = false;
        }

        private void EnableAllInput()
        {
            this.WidthText.IsEnabled = true;
            this.HeightText.IsEnabled = true;
            this.PositionXText.IsEnabled = true;
            this.PositionYText.IsEnabled = true;
            this.ArticleText.IsEnabled = true;
            this.QuantityText.IsEnabled = true;
            this.CapacityText.IsEnabled = true;
            this.CreateCompartment.IsEnabled = true;
        }

        #endregion Methods
    }
}
