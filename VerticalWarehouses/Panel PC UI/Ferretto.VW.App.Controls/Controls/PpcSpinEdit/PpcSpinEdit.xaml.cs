﻿using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;
using Ferretto.VW.App.Controls.Controls.Keyboards;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Interfaces;

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

        public static readonly DependencyProperty KeyboardProperty = DependencyProperty.Register(
                            nameof(Keyboard),
            typeof(KeyboardType),
            typeof(PpcSpinEdit),
            new PropertyMetadata(KeyboardType.NumpadCenter));

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

        private const string DECIMAL_STYLE = "VWAPP_SpinEdit_DecimalStyle";

        private const string DOUBLE_STYLE = "VWAPP_SpinEdit_DoubleStyle";

        private const string INTEGER_STYLE = "VWAPP_SpinEdit_IntegerStyle";

        private static readonly DependencyProperty EditValueProperty = DependencyProperty.Register(
            nameof(EditValue),
            typeof(object),
            typeof(PpcSpinEdit),
            new FrameworkPropertyMetadata(null));

        private bool isStyleSet;

        #endregion

        #region Constructors

        public PpcSpinEdit()
        {
            this.InitializeComponent();
            var customInputFieldControlFocusable = this;
            this.LayoutRoot.DataContext = customInputFieldControlFocusable;
            this.Loaded += this.PpcSpinEdit_Loaded;
        }

        #endregion

        #region Properties

        public SolidColorBrush BorderColor
        {
            get => (SolidColorBrush)this.GetValue(BorderColorProperty);
            set => this.SetValue(BorderColorProperty, value);
        }

        public object EditValue
        {
            get => this.GetValue(EditValueProperty);
            set => this.SetValue(EditValueProperty, value);
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

        public bool IsStyleSet => this.isStyleSet;

        public KeyboardType Keyboard
        {
            get => (KeyboardType)this.GetValue(KeyboardProperty);
            set => this.SetValue(KeyboardProperty, value);
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

        public void SetStyle(string styleName)
        {
            var style = Application.Current.Resources[styleName] as Style;
            this.InnerSpinEdit.Style = style;
            this.isStyleSet = true;
        }

        private static PropertyInfo GetProperty(Type type, string fieldPathName)
        {
            var pathTokens = fieldPathName.Split('.');
            PropertyInfo property = null;
            foreach (var memberName in pathTokens)
            {
                property = type.GetProperty(memberName);
                if (property == null)
                {
                    System.Diagnostics.Debug.Print(
                        $"Cannot retrieve property '{fieldPathName}' because property '{memberName}' is not available on type '{type}'.");

                    return null;
                }

                type = property.PropertyType;
            }

            if (property == null)
            {
                System.Diagnostics.Debug.Print(
                    $"Cannot retrieve property '{fieldPathName}' for model type '{type}'.");

                return null;
            }

            return property;
        }

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
                var b = this.GetBindingExpression(EditValueProperty);
                b?.UpdateSource();
            }
        }

        private void OnMouseDoubleClickHandler(object sender, MouseButtonEventArgs e)
        {
            switch (this.Keyboard)
            {
                case KeyboardType.NumpadCenter:
                    var ppcMessagePopup = new PpcNumpadCenterPopup();
                    var vm = new PpcKeypadsPopupViewModel();
                    ppcMessagePopup.DataContext = vm;
                    vm.Update(this.LabelText, this.EditValue?.ToString() ?? string.Empty);
                    ppcMessagePopup.Topmost = false;
                    ppcMessagePopup.ShowInTaskbar = false;
                    PpcMessagePopup.ShowDialog(ppcMessagePopup);
                    this.EditValue = vm.ScreenText;
                    break;

                case KeyboardType.Numpad:
                    var ppcNumpadPopup = new PpcNumpadPopup();
                    var vmNumpad = new PpcKeypadsPopupViewModel();
                    ppcNumpadPopup.DataContext = vmNumpad;
                    vmNumpad.Update(this.LabelText, this.EditValue?.ToString() ?? string.Empty);
                    ppcNumpadPopup.Topmost = false;
                    ppcNumpadPopup.ShowInTaskbar = false;
                    PpcMessagePopup.ShowAnchorDialog(ppcNumpadPopup);
                    this.EditValue = vmNumpad.ScreenText;
                    break;

                default:
                    break;
            }
        }

        private void PpcSpinEdit_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsStyleSet)
            {
                return;
            }

            var bindingExpression = BindingOperations.GetBindingExpression(this, PpcSpinEdit.EditValueProperty);

            var dataContextType = this.DataContext.GetType();
            var path = bindingExpression.ParentBinding.Path.Path;

            var property = GetProperty(dataContextType, path);

            if (property is null)
            {
                return;
            }

            var type = property.PropertyType;
            if (Nullable.GetUnderlyingType(property.PropertyType) is Type nullType)
            {
                type = nullType;
            }

            if (type == typeof(int))
            {
                this.SetStyle(INTEGER_STYLE);
                return;
            }

            if (type == typeof(double))
            {
                this.SetStyle(DOUBLE_STYLE);
                return;
            }

            if (type == typeof(decimal))
            {
                this.SetStyle(DECIMAL_STYLE);
                return;
            }
        }

        #endregion
    }
}
