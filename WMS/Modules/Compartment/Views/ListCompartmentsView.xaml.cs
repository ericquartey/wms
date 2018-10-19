using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.Compartment
{
    /// <summary>
    /// Interaction logic for ListCompartmentsView.xaml
    /// </summary>
    public partial class ListCompartmentsView : WmsView
    {
        #region Fields

        public static readonly DependencyProperty CompartmentsProperty = DependencyProperty.Register(
                    nameof(Compartments), typeof(IList<CompartmentDetails>), typeof(ListCompartmentsView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCompartmentsChanged)));

        private IList<CompartmentDetails> compartments;

        #endregion Fields

        #region Constructors

        public ListCompartmentsView()
        {
            this.InitializeComponent();

            this.GridCompartment.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            var label = new Label();
            label.Content = "1";
            this.GridCompartment.Children.Add(label);
            Grid.SetRow(label, this.GridCompartment.RowDefinitions.Count - 1);
            Grid.SetColumn(label, 0);
            this.GridCompartment.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            label = new Label();
            label.Content = "2";
            this.GridCompartment.Children.Add(label);
            Grid.SetRow(label, this.GridCompartment.RowDefinitions.Count - 1);
            Grid.SetColumn(label, 0);

            //this.TestInitializeGrid();
        }

        #endregion Constructors

        #region Properties

        public IList<CompartmentDetails> Compartments
        {
            get { return this.compartments; }
            set { this.SetValue(CompartmentsProperty, value); }
        }

        #endregion Properties

        #region Methods

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            //this.MainGridControl.CurrentDataSource = Compartments;
            TestInitializeGrid();
        }

        private static void OnCompartmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListCompartmentsView listCompartments && listCompartments.DataContext is ListCompartmentViewModel viewModel)
            {
                viewModel.UpdateGridList((IList<CompartmentDetails>)e.NewValue);
            }
        }

        private void TestInitializeGrid()
        {
            this.Compartments = new List<CompartmentDetails>()
            {
                new CompartmentDetails()
                {
                    Code = "1",
                    XPosition = 0,
                    YPosition = 0,
                    Width = 150,
                    Height = 150
                }
            };
        }

        #endregion Methods
    }
}
