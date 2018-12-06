using System;
using System.Windows;
using System.Windows.Media;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.Common.Controls
{
    public class InfoText : System.Windows.Controls.Label
    {
        #region Fields

        public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register(
            nameof(BackgroundBrush), typeof(SolidColorBrush), typeof(InfoText), new PropertyMetadata(default(SolidColorBrush)));

        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register(
            nameof(ContentText), typeof(object), typeof(InfoText), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
            nameof(EnumType), typeof(Type), typeof(InfoText), new PropertyMetadata(default(Type)));

        public static readonly DependencyProperty IsPillVisibleProperty = DependencyProperty.Register(
            nameof(IsPillVisible), typeof(bool), typeof(InfoText), new PropertyMetadata(default(bool)));

        #endregion Fields

        #region Properties

        public SolidColorBrush BackgroundBrush
        {
            get => (SolidColorBrush) this.GetValue(BackgroundBrushProperty);
            set => this.SetValue(BackgroundBrushProperty, value);
        }

        public object ContentText
        {
            get => this.GetValue(ContentTextProperty);
            set => this.SetValue(ContentTextProperty, value);
        }

        public Type EnumType
        {
            get => (Type) this.GetValue(EnumTypeProperty);
            set => this.SetValue(EnumTypeProperty, value);
        }

        public bool IsPillVisible
        {
            get => (bool) this.GetValue(IsPillVisibleProperty);
            set => this.SetValue(IsPillVisibleProperty, value);
        }

        #endregion Properties

        #region Methods

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property != ContentProperty)
            {
                return;
            }

            var bindingExpression = this.GetBindingExpression(ContentProperty);
            var propertyInfo = bindingExpression?.ResolvedSource.GetType()
                .GetProperty(bindingExpression.ResolvedSourcePropertyName);

            if (propertyInfo?.PropertyType.IsEnum == true)
            {
                this.IsPillVisible = true;
                this.EnumType = propertyInfo.PropertyType;

                var resourceValue = this.SymbolName != null
                    ? EnumColors.ResourceManager.GetString(this.SymbolName)
                    : null;

                this.BackgroundBrush = resourceValue != null
                    ? new SolidColorBrush((Color) ColorConverter.ConvertFromString(resourceValue))
                    : Brushes.Transparent;

                this.ContentText = (this.Content as Enum).GetDisplayName(this.EnumType);
            }
            else
            {
                this.IsPillVisible = false;
                this.ContentText = this.Content;
            }
        }

        public string SymbolName
        {
            get
            {
                if (this.Content == null || this.EnumType == null || this.Content is Enum == false)
                {
                    return null;
                }

                var enumValue = Enum.GetName(this.EnumType, this.Content);
                return $"{this.EnumType.Name}{enumValue}";
            }
        }

        #endregion Methods
    }
}
