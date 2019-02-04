using System.Windows.Controls;
using DevExpress.Data.Async.Helpers;

namespace Ferretto.Common.Controls
{
    public class CustomBorder : Border
    {
        #region Fields

        private object dataContext;

        #endregion

        #region Constructors

        public CustomBorder()
        {
            this.DataContextChanged += this.CustomBorder_DataContextChanged;
            this.Loaded += this.CustomGrid_Loaded;
        }

        #endregion

        #region Methods

        private void CustomBorder_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                this.dataContext = e.NewValue;
            }
            this.DataContext = null;
            this.DataContextChanged -= this.CustomBorder_DataContextChanged;
        }

        private void CustomGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.dataContext is ReadonlyThreadSafeProxyForObjectFromAnotherThread dataContext)
            {
                var originalRow = dataContext.OriginalRow;
                this.DataContext = originalRow;
            }
        }

        #endregion
    }
}
