using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls
{
    public class SpinEdit : DevExpress.Xpf.Editors.SpinEdit, ITitleControl
    {
        #region Fields

        public static readonly DependencyProperty EnableCorrectionIncrementProperty = DependencyProperty.Register(
                    nameof(EnableCorrection), typeof(bool), typeof(TextBox), new PropertyMetadata(default(bool)));

        #endregion

        #region Constructors

        public SpinEdit()
        {
            this.Loaded += this.SpinEdit_Loaded;
        }

        #endregion

        #region Properties

        public bool EnableCorrection
        {
            get => (bool)this.GetValue(EnableCorrectionIncrementProperty);
            set => this.SetValue(EnableCorrectionIncrementProperty, value);
        }

        #endregion

        #region Methods

        private void ApplyCorrection()
        {
            if (this.EnableCorrection && this.Value > 0)
            {
                this.EditValue = (double)(Math.Floor(this.Value / this.Increment) * this.Increment);
            }
        }

        private void EditCore_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.ApplyCorrection();
        }

        private void OnMouseDown(MouseDownInfo mouseDownInfo)
        {
            if (mouseDownInfo.OriginalSource is UIElement uiElement &&
                this.EnableCorrection)
            {
                if (LayoutTreeHelper.GetVisualParents(uiElement).OfType<SpinEdit>().FirstOrDefault() != null)
                {
                    if (LayoutTreeHelper.GetVisualParents((DependencyObject)mouseDownInfo.OriginalSource).OfType<ButtonsControl>().FirstOrDefault() != null)
                    {
                        this.ApplyCorrection();
                    }
                }
                else
                {
                    this.ApplyCorrection();
                }
            }
        }

        private void SpinEdit_Loaded(object sender, RoutedEventArgs e)
        {
            this.EditCore.PreviewLostKeyboardFocus += this.EditCore_PreviewLostKeyboardFocus;

            var inputService = ServiceLocator.Current.GetInstance<IInputService>();
            inputService.BeginMouseNotify(this, this.OnMouseDown);
        }

        #endregion
    }
}
