using System;
using System.Windows;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;

namespace Ferretto.WMS.Modules.MasterData
{
    /// <summary>
    /// Interaction logic for InputBulkAddCompartmentView.xaml
    /// </summary>
    public partial class InputBulkAddCompartmentView : WmsView
    {
        //public static readonly DependencyProperty EnableProperty = DependencyProperty.Register(
        //    nameof(Enable), typeof(bool), typeof(InputBulkAddCompartmentView), new FrameworkPropertyMetadata(false, OnEnableChanged));

        ////public static readonly DependencyProperty EnableInternalProperty = DependencyProperty.Register(
        ////    nameof(EnableInternal), typeof(bool), typeof(InputBulkAddCompartmentView), new FrameworkPropertyMetadata(false, OnEnableInternalChanged));

        //public static readonly DependencyProperty TrayProperty = DependencyProperty.Register(
        //    nameof(Tray), typeof(Tray), typeof(InputBulkAddCompartmentView), new FrameworkPropertyMetadata(OnTrayChanged));

        #region Constructors

        public InputBulkAddCompartmentView()
        {
            this.InitializeComponent();
            //this.DataContext = new InputBulkAddCompartmentViewModel();
        }

        #endregion Constructors

        //public bool Enable
        //{
        //    get => (bool)this.GetValue(EnableProperty);
        //    set => this.SetValue(EnableProperty, value);
        //}

        //public bool EnableInternal
        //{
        //    get => (bool)this.GetValue(EnableInternalProperty);
        //    set => this.SetValue(EnableInternalProperty, value);
        //}

        //public Tray Tray
        //{
        //    get => (Tray)this.GetValue(TrayProperty);
        //    set => this.SetValue(TrayProperty, value);
        //}

        //private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is InputBulkAddCompartmentView inputBulkAdd && inputBulkAdd.DataContext is InputBulkAddCompartmentViewModel viewModel)
        //    {
        //        var enable = (bool)e.NewValue;
        //        //if (enable)
        //        //{
        //        //    inputBulkAdd.Visibility = Visibility.Visible;
        //        //}
        //        //else
        //        //{
        //        //    inputBulkAdd.Visibility = Visibility.Collapsed;
        //        //}
        //        // inputBulkAdd.EnableInternal = enable;
        //        viewModel.EnableInputBulkAdd = (bool)e.NewValue;
        //    }
        //}

        //private static void OnEnableInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is InputBulkAddCompartmentView inputBulkAdd && inputBulkAdd.DataContext is InputBulkAddCompartmentViewModel viewModel)
        //    {
        //        var enable = (bool)e.NewValue;
        //        if (enable)
        //        {
        //            inputBulkAdd.Visibility = Visibility.Visible;
        //        }
        //        else
        //        {
        //            inputBulkAdd.Visibility = Visibility.Collapsed;
        //        }
        //        //inputBulkAdd.EnableExternal = enable;
        //        //viewModel.BulkAddVisibility = (bool)e.NewValue;
        //    }
        //}

        //private static void OnTrayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is InputBulkAddCompartmentView inputBulkAdd && inputBulkAdd.DataContext is InputBulkAddCompartmentViewModel viewModel)
        //    {
        //        viewModel.Tray = (Tray)e.NewValue;
        //    }
        //}

        //private void InputBulkAddCompartmentView_IsVisibleChanged(Object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (!(bool)e.NewValue)
        //    {
        //        this.Visibility = Visibility.Collapsed;
        //        this.Enable = false;
        //    }
        //    else
        //    {
        //        this.Visibility = Visibility.Visible;
        //    }
        //}

        //private bool enable;
    }
}
