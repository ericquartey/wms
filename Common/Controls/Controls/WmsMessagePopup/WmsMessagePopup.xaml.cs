using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Ferretto.Common.Controls
{
    /// <summary>
    /// Interaction logic for WmsMessagePopup.xaml
    /// </summary>
    public partial class WmsMessagePopup : WmsDialogView
    {
        #region Fields

        public static readonly DependencyProperty IsErrorProperty = DependencyProperty.Register(
                                nameof(IsError),
                                typeof(bool),
                                typeof(WmsMessagePopup),
                                new FrameworkPropertyMetadata(false, OnIsErrorChanged));

        private Storyboard sbShowConnected;

        private Storyboard sbShowErrorConnection;

        #endregion

        #region Constructors

        public WmsMessagePopup()
        {
            this.InitializeComponent();
            Application.Current.MainWindow.ResizeMode = ResizeMode.NoResize;
        }

        #endregion

        #region Properties

        public bool IsError
        {
            get => (bool)this.GetValue(IsErrorProperty);
            set => this.SetValue(IsErrorProperty, value);
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.sbShowErrorConnection = this.Resources[nameof(WmsMessagePopup.sbShowErrorConnection)] as Storyboard;
            this.sbShowConnected = this.Resources[nameof(WmsMessagePopup.sbShowConnected)] as Storyboard;
            this.sbShowConnected.Completed += this.SbShowConnected_Completed;
        }

        protected override void OnDataContextLoaded()
        {
            base.OnDataContextLoaded();

            Binding binding = new Binding();
            binding.Path = new PropertyPath(nameof(WmsMessagePopupViewModel.IsError), null);
            binding.Source = this.DataContext;
            BindingOperations.SetBinding(this, WmsMessagePopup.IsErrorProperty, binding);
        }

        private static void OnIsErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsMessagePopup messagePopup &&
                e.NewValue is bool isError)
            {
                if (isError)
                {
                    messagePopup.sbShowErrorConnection.Begin(messagePopup.thunderPath);
                }
                else
                {
                    messagePopup.sbShowConnected.Begin(messagePopup.plugPath);
                }
            }
        }

        private void SbShowConnected_Completed(object sender, EventArgs e)
        {
            if (this.IsError == false)
            {
                this.sbShowConnected.Completed -= this.SbShowConnected_Completed;
                this.Close();
                Application.Current.MainWindow.ResizeMode = ResizeMode.CanResize;
            }
        }

        #endregion
    }
}
