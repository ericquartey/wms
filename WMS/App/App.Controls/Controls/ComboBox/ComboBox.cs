using System.Windows;
using DevExpress.Mvvm;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Controls
{
    public class ComboBox : DevExpress.Xpf.Editors.ComboBoxEdit
    {
        #region Fields

        public static readonly DependencyProperty BusinessObjectValueProperty = DependencyProperty.Register(
            nameof(BusinessObjectValue),
            typeof(object),
            typeof(ComboBox),
            new UIPropertyMetadata(OnBusinessObjectValuePropertyChanged));

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
            if (newValue is EnumMemberInfo info)
            {
                this.BusinessObjectValue = info.Id;
            }
            else if (newValue is IModel<int> model)
            {
                this.BusinessObjectValue = model.Id;
            }
            else if (newValue == null)
            {
                this.BusinessObjectValue = null;
            }
        }

        private static void OnBusinessObjectValuePropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
                if (e.NewValue == null)
                {
                    comboBox.EditValue = null;
                    return;
                }

                if (e.NewValue is int newValue)
                {
                    comboBox.EditValue = newValue;
                }
            }
        }

        #endregion
    }
}
