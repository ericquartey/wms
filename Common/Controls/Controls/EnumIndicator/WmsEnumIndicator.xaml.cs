using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.Common.Controls
{
    public partial class WmsEnumIndicator : UserControl
    {
        #region Fields

        public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
            nameof(EnumType), typeof(Type), typeof(WmsEnumIndicator), new PropertyMetadata());

        public static readonly DependencyProperty EnumValueProperty = DependencyProperty.Register(
            nameof(EnumValue), typeof(object), typeof(WmsEnumIndicator), new PropertyMetadata());

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

                var resource = this.FindResource(resourceName);
                if (resource is Brush brushResource)
                {
                    return brushResource;
                }
                else if (resource is Color colorResource)
                {
                    return new SolidColorBrush(colorResource);
                }
                else
                {
                    throw new Exception(
                        $"Expected resource {resourceName} to be of type {nameof(Brush)} or {nameof(Color)}, but it has type {resource.GetType().Name} instead.");
                }
            }
        }

        public Type EnumType
        {
            get => (Type)this.GetValue(EnumTypeProperty);
            set => this.SetValue(EnumTypeProperty, value);
        }

        public object EnumValue
        {
            get => this.GetValue(EnumValueProperty);
            set => this.SetValue(EnumValueProperty, value);
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
    }
}
