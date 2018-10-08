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
    /// <summary>
    /// Interaction logic for WmsCompartmentControl.xaml
    /// </summary>
    public partial class WmsCompartmentControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CompartmentProperty = DependencyProperty.Register(
         nameof(Compartment), typeof(CompartmentDetails), typeof(WmsCompartmentControl), new PropertyMetadata(default(CompartmentDetails), new PropertyChangedCallback(OnCompartmentChanged)));

        #endregion Fields

        #region Constructors

        public WmsCompartmentControl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public CompartmentDetails Compartment
        {
            get => (CompartmentDetails)this.GetValue(CompartmentProperty);
            set => this.SetValue(CompartmentProperty, value);
        }

        #endregion Properties

        #region Methods

        public void UpdateCompartment(WmsCompartment comp)
        {
            var dataContext = this.DataContext as WmsCompartmentViewModel;
            if (dataContext != null)
            {
                dataContext.UpdateCompartment(comp);
            }
        }

        private static void OnCompartmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCompartmentControl wmsCompartment)
            {
                if (wmsCompartment.DataContext is WmsCompartmentViewModel viewModel)
                {
                    viewModel.Refresh((CompartmentDetails)e.NewValue);
                }
            }
        }

        #endregion Methods
    }
}
