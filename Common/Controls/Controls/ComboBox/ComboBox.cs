using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Xpf.Editors;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class ComboBox : DevExpress.Xpf.Editors.ComboBoxEdit
    {
        #region Fields

        public static readonly DependencyProperty BusinessObjectValueProperty = DependencyProperty.Register(
            nameof(BusinessObjectValue),
            typeof(object),
            typeof(ComboBox),
            new UIPropertyMetadata(null));

        #endregion

        #region Properties

        public object BusinessObjectValue
        {
            get => this.GetValue(BusinessObjectValueProperty);
            set => this.SetValue(BusinessObjectValueProperty, value);
        }

        #endregion

        #region Methods

        protected override void OnSelectedItemChanged(object oldValue, object newValue)
        {
            base.OnSelectedItemChanged(oldValue, newValue);
            var type = newValue?.GetType();
            if (type == typeof(Enumeration))
            {
                var enumeration = (Enumeration)newValue;
                this.BusinessObjectValue = enumeration.Id;
            }
            else if (type == typeof(EnumMemberInfo))
            {
                var enumInfo = (EnumMemberInfo)newValue;
                this.BusinessObjectValue = enumInfo.Id;
            }
        }

        #endregion
    }
}
