using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils.Extensions;

namespace Ferretto.Common.Controls
{
    public partial class WmsEnumIndicator : UserControl
    {
        #region Fields

        public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register(
            nameof(BackgroundBrush), typeof(SolidColorBrush), typeof(WmsEnumIndicator), new PropertyMetadata(default(SolidColorBrush)));

        public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
            nameof(EnumType), typeof(Type), typeof(WmsEnumIndicator), new PropertyMetadata(new PropertyChangedCallback(EnumChanged)));

        public static readonly DependencyProperty EnumValueProperty = DependencyProperty.Register(
            nameof(EnumValue), typeof(Enum), typeof(WmsEnumIndicator), new PropertyMetadata(new PropertyChangedCallback(EnumChanged)));

        public static readonly DependencyProperty HideIconProperty = DependencyProperty.Register(
            nameof(HideIcon), typeof(bool), typeof(WmsEnumIndicator), new PropertyMetadata(new PropertyChangedCallback(EnumChanged)));

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
                    nameof(IconSize), typeof(int), typeof(WmsEnumIndicator));

        public static readonly DependencyProperty ShowTextProperty = DependencyProperty.Register(
                    nameof(ShowText), typeof(bool), typeof(WmsEnumIndicator), new PropertyMetadata(new PropertyChangedCallback(ShowTextChanged)));

        public static readonly DependencyProperty SymbolNameProperty = DependencyProperty.Register(
            nameof(SymbolName), typeof(string), typeof(WmsEnumIndicator), new PropertyMetadata(default(string)));

        #endregion

        #region Constructors

        public WmsEnumIndicator()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public SolidColorBrush BackgroundBrush
        {
            get => (SolidColorBrush)this.GetValue(BackgroundBrushProperty);
            set => this.SetValue(BackgroundBrushProperty, value);
        }

        public Type EnumType
        {
            get => (Type)this.GetValue(EnumTypeProperty);
            set => this.SetValue(EnumTypeProperty, value);
        }

        public Enum EnumValue
        {
            get => (Enum)this.GetValue(EnumValueProperty);
            set => this.SetValue(EnumValueProperty, value);
        }

        public bool HideIcon
        {
            get => (bool)this.GetValue(HideIconProperty);
            set => this.SetValue(HideIconProperty, value);
        }

        public int IconSize
        {
            get => (int)this.GetValue(IconSizeProperty);
            set => this.SetValue(IconSizeProperty, value);
        }

        public bool ShowText
        {
            get => (bool)this.GetValue(ShowTextProperty);
            set => this.SetValue(ShowTextProperty, value);
        }

        public string SymbolName
        {
            get => (string)this.GetValue(SymbolNameProperty);
            set => this.SetValue(SymbolNameProperty, value);
        }

        #endregion

        #region Methods

        private static void EnumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is WmsEnumIndicator instance))
            {
                return;
            }

            UpdateText(instance);

            if (instance.EnumValue == null || instance.EnumType == null)
            {
                instance.BackgroundBrush = Brushes.Transparent;
                return;
            }

            var enumValue = Enum.GetName(instance.EnumType, instance.EnumValue);
            instance.SymbolName = $"{instance.EnumType.Name}{enumValue}";

            var resourceValue = EnumColors.ResourceManager.GetString(instance.SymbolName);

            instance.BackgroundBrush = resourceValue != null
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(resourceValue))
                : Brushes.Transparent;
        }

        private static void ShowTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateText(d as WmsEnumIndicator);
        }

        private static void UpdateText(WmsEnumIndicator control)
        {
            if (control == null)
            {
                return;
            }

            control.textBlock.Text = control.EnumValue != null && control.EnumType != null && control.ShowText ? control.EnumValue.GetDisplayName(control.EnumType) : null;
        }

        #endregion
    }
}
