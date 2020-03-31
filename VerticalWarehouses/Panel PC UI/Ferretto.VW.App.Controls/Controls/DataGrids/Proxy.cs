using System.Windows;

namespace Ferretto.VW.App.Controls
{
    public class BindingProxy : Freezable
    {
        #region Fields

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        #endregion

        #region Properties

        public object Data
        {
            get => (object)this.GetValue(DataProperty);
            set => this.SetValue(DataProperty, value);
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
