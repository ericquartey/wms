using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;

namespace Ferretto.Common.Controls
{
    public class ComboBox : DevExpress.Xpf.Editors.ComboBoxEdit
    {
        #region Methods

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //EnumHelper.GetAttributeOfType();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property != EditValueProperty)
            {
                return;
            }

            var bindingExpression = this.GetBindingExpression(EditValueProperty);

            if (bindingExpression?.ResolvedSourcePropertyName == null)
            {
                return;
            }

            var propertyInfo = bindingExpression?.ResolvedSource?.GetType()
                .GetProperty(bindingExpression.ResolvedSourcePropertyName);

            if (propertyInfo?.PropertyType.IsEnum == true)
            {
                //this.EnumType = propertyInfo.PropertyType;

                //var resourceValue = this.SymbolName != null
                //    ? EnumColors.ResourceManager.GetString(this.SymbolName)
                //    : null;

                //this.BackgroundBrush = resourceValue != null
                //    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(resourceValue))
                //    : Brushes.Transparent;

                //this.ContentText = (this.Content as Enum).GetDisplayName(this.EnumType);
            }
            else
            {
                //this.ContentText = this.Content;
            }
            if (e.Property.ToString() == "ItemsSource")
            {
                var output = EnumHelper.GetDisplayValues(e.GetType());
                var key = output["Key"];
                var valeu = output["Value"];
            }
        }

        #endregion Methods

        //settings.DataSource = typeof(BusinessTripStates).GetDisplayValues();
        // settings.ValueField = "Key";
        //settings.TextField = "Value";
    }
}
