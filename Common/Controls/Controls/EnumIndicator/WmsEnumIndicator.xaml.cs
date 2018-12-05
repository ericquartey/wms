using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.Common.Controls
{
    public partial class WmsEnumIndicator : UserControl
    {
        #region Fields

        public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
            nameof(EnumType), typeof(Type), typeof(WmsEnumIndicator), new PropertyMetadata(new PropertyChangedCallback(EnumChanged)));

        public static readonly DependencyProperty EnumValueProperty = DependencyProperty.Register(
            nameof(EnumValue), typeof(Enum), typeof(WmsEnumIndicator), new PropertyMetadata(new PropertyChangedCallback(EnumChanged)));

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
            nameof(IconSize), typeof(int), typeof(WmsEnumIndicator));

        public static readonly DependencyProperty ShowTextProperty = DependencyProperty.Register(
            nameof(ShowText), typeof(bool), typeof(WmsEnumIndicator), new PropertyMetadata(new PropertyChangedCallback(ShowTextChanged)));

        #endregion Fields

        #region Constructors

        public WmsEnumIndicator()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public Brush BackgroundBrush
        {
            get
            {
                var resourceName = this.SymbolName;
                if (resourceName == null)
                {
                    return Brushes.Transparent;
                }

                var resourceValue = EnumColors.ResourceManager.GetString(resourceName);

                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(resourceValue));
            }
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
            get
            {
                if (this.EnumValue == null || this.EnumType == null)
                {
                    return null;
                }

                var enumValue = Enum.GetName(this.EnumType, this.EnumValue);
                return $"{this.EnumType.Name}{enumValue}";
            }
        }

        #endregion Properties

        #region Methods

        private static void EnumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateText(d as WmsEnumIndicator);
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

            if (control.EnumValue != null && control.EnumType != null && control.ShowText)
            {
                control.textBlock.Text = control.EnumValue.GetDisplayName(control.EnumType);
            }
            else
            {
                control.textBlock.Text = null;
            }
        }

        #endregion Methods
    }
}
