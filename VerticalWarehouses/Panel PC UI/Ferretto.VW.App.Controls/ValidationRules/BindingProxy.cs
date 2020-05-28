using System.Windows;

namespace Ferretto.VW.App.Controls.ValidationRules
{
    public class BindingProxy : Freezable
    {
        #region Fields

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new PropertyMetadata(null));

        #endregion

        #region Properties

        public object Data
        {
            get { return (object)this.GetValue(DataProperty); }
            set { this.SetValue(DataProperty, value); }
        }

        #endregion

        #region Methods

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion
    }
}
