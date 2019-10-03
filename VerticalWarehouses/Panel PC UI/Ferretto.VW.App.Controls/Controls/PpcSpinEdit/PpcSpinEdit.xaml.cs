using System.Windows;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;

namespace Ferretto.VW.App.Controls
{
    public partial class PpcSpinEdit : UserControl
    {
        #region Fields

        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register(
            nameof(BorderColor),
            typeof(SolidColorBrush),
            typeof(PpcSpinEdit));

        public static readonly DependencyProperty HighlightedProperty = DependencyProperty.Register(
            nameof(Highlighted),
            typeof(bool),
            typeof(PpcSpinEdit));

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment),
            typeof(decimal),
            typeof(PpcSpinEdit),
            new PropertyMetadata(new decimal(1), new PropertyChangedCallback(OnIncrementChanged)));

        public static readonly DependencyProperty InputTextProperty = DependencyProperty.Register(
            nameof(InputText),
            typeof(string),
            typeof(PpcSpinEdit),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
            nameof(LabelText),
            typeof(string),
            typeof(PpcSpinEdit),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
            nameof(Mask),
            typeof(string),
            typeof(PpcSpinEdit),
            new PropertyMetadata(null, new PropertyChangedCallback(OnMaskChanged)));

        public static readonly DependencyProperty SpinEditStyleProperty = DependencyProperty.Register(
            nameof(SpinEditStyle),
            typeof(Style),
            typeof(PpcSpinEdit),
            new PropertyMetadata(null, new PropertyChangedCallback(OnSpinEditStyleChanged)));

        public static readonly DependencyProperty WidthNumberProperty = DependencyProperty.Register(
            nameof(WidthNumber),
            typeof(decimal),
            typeof(PpcSpinEdit),
            new PropertyMetadata(new decimal(250)));

        #endregion

        #region Constructors

        public PpcSpinEdit()
        {
            try
            {
                this.InitializeComponent();
                var customInputFieldControlFocusable = this;
                this.LayoutRoot.DataContext = customInputFieldControlFocusable;
            }
            catch (System.Exception)
            {
            }
        }

        #endregion

        #region Properties

        public SolidColorBrush BorderColor
        {
            get => (SolidColorBrush)this.GetValue(BorderColorProperty);
            set => this.SetValue(BorderColorProperty, value);
        }

        public bool Highlighted
        {
            get => (bool)this.GetValue(HighlightedProperty);
            set => this.SetValue(HighlightedProperty, value);
        }

        public decimal Increment
        {
            get => (decimal)this.GetValue(IncrementProperty);
            set => this.SetValue(IncrementProperty, value);
        }

        public string InputText
        {
            get => (string)this.GetValue(InputTextProperty);
            set => this.SetValue(InputTextProperty, value);
        }

        public string LabelText
        {
            get => (string)this.GetValue(LabelTextProperty);
            set => this.SetValue(LabelTextProperty, value);
        }

        public string Mask
        {
            get => (string)this.GetValue(MaskProperty);
            set => this.SetValue(MaskProperty, value);
        }

        public Style SpinEditStyle
        {
            get => (Style)this.GetValue(SpinEditStyleProperty);
            set => this.SetValue(SpinEditStyleProperty, value);
        }

        public decimal WidthNumber
        {
            get => (decimal)this.GetValue(WidthNumberProperty);
            set => this.SetValue(WidthNumberProperty, value);
        }

        #endregion

        #region Methods

        private static void OnIncrementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcSpinEdit ppcSpinEdit
                &&
                e.NewValue is decimal increment)
            {
                ppcSpinEdit.InnerSpinEdit.Increment = increment;
            }
        }

        private static void OnMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcSpinEdit ppcSpinEdit
                &&
                e.NewValue is string mask)
            {
                ppcSpinEdit.InnerSpinEdit.Mask = mask;
                ppcSpinEdit.InnerSpinEdit.MaskUseAsDisplayFormat = true;
            }
            else if (d is PpcSpinEdit se)
            {
                se.InnerSpinEdit.Mask = string.Empty;
                se.InnerSpinEdit.MaskUseAsDisplayFormat = false;
            }
        }

        private static void OnSpinEditStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PpcSpinEdit ppcSpinEdit
                &&
                e.NewValue is Style style)
            {
                ppcSpinEdit.InnerSpinEdit.Style = style;
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var b = this.GetBindingExpression(InputTextProperty);
                b?.UpdateSource();
            }
        }

        private void OnPreviewEventDown(object sender, InputEventArgs e)
        {
            var spinEdit = (SpinEdit)sender;
            if (LayoutTreeHelper.GetVisualParents((DependencyObject)e.OriginalSource, spinEdit).Any(x => x is TextBox))
            {
                this.Dispatcher.BeginInvoke((Action)spinEdit.SelectAll);
            }
        }

        #endregion
    }
}
