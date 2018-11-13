using System;
using System.Windows;
using System.Windows.Controls;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    /// <summary>
    /// Interaction logic for WmsInputCompartmentView.xaml
    /// </summary>
    public partial class InputCompartmentView : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
                    nameof(Column), typeof(int), typeof(InputCompartmentView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnColumnChanged)));

        public static readonly DependencyProperty EnableBulkAddProperty = DependencyProperty.Register(
                            nameof(EnableBulkAdd), typeof(bool), typeof(InputCompartmentView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnEnableBulkAddChanged)));

        public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
                    nameof(Row), typeof(int), typeof(InputCompartmentView), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnRowChanged)));

        #endregion Fields

        #region Constructors

        public InputCompartmentView()
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
            if (d is InputCompartmentView inputCompartment)
            {
                inputCompartment.Column = (int)e.NewValue;
            }
        }

        private static void OnEnableBulkAddChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InputCompartmentView inputCompartment)
            {
                inputCompartment.EnableBulkAddVisibility((bool)e.NewValue);
            }
        }

        private static void OnRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InputCompartmentView inputCompartment)
            {
                inputCompartment.Row = (int)e.NewValue;
            }
        }

        #endregion Methods

        //private void DisenableAllInput()
        //{
        //    this.WidthText.IsEnabled = false;
        //    this.HeightText.IsEnabled = false;
        //    this.PositionXText.IsEnabled = false;
        //    this.PositionYText.IsEnabled = false;
        //    this.ArticleText.IsEnabled = false;
        //    this.QuantityText.IsEnabled = false;
        //    this.CapacityText.IsEnabled = false;
        //    //this.CreateCompartment.IsEnabled = false;
        //}

        //private void EnableAllInput()
        //{
        //    this.WidthText.IsEnabled = true;
        //    this.HeightText.IsEnabled = true;
        //    this.PositionXText.IsEnabled = true;
        //    this.PositionYText.IsEnabled = true;
        //    this.ArticleText.IsEnabled = true;
        //    this.QuantityText.IsEnabled = true;
        //    this.CapacityText.IsEnabled = true;
        //    //this.CreateCompartment.IsEnabled = true;
        //}
    }
}
