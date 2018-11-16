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

namespace Ferretto.WMS.Modules.MasterData
{
    /// <summary>
    /// Interaction logic for InputBulkAddCompartmentView.xaml
    /// </summary>
    public partial class InputBulkAddCompartmentView : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
                    nameof(Column), typeof(int), typeof(InputBulkAddCompartmentView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnColumnChanged)));

        public static readonly DependencyProperty EnableBulkAddProperty = DependencyProperty.Register(
                            nameof(EnableBulkAdd), typeof(bool), typeof(InputBulkAddCompartmentView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnEnableBulkAddChanged)));

        public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
                    nameof(Row), typeof(int), typeof(InputBulkAddCompartmentView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRowChanged)));

        #endregion Fields

        #region Constructors

        public InputBulkAddCompartmentView()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public int Column { get; set; }

        public bool EnableBulkAdd { get; set; }

        public int Row { get; set; }

        #endregion Properties

        #region Methods

        public void EnableBulkAddVisibility(bool enable)
        {
            if (enable)
            {
                this.ColumnText.Visibility = Visibility.Visible;
                this.RowText.Visibility = Visibility.Visible;
            }
            else
            {
                this.ColumnText.Visibility = Visibility.Collapsed;
                this.RowText.Visibility = Visibility.Collapsed;
            }
        }

        private static void OnColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InputBulkAddCompartmentView inputCompartment)
            {
                inputCompartment.Column = (int)e.NewValue;
            }
        }

        private static void OnEnableBulkAddChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InputBulkAddCompartmentView inputCompartment)
            {
                inputCompartment.EnableBulkAddVisibility((bool)e.NewValue);
            }
        }

        private static void OnRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InputBulkAddCompartmentView inputCompartment)
            {
                inputCompartment.Row = (int)e.NewValue;
            }
        }

        #endregion Methods
    }
}
