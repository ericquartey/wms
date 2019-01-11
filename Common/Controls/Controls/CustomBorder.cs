using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DevExpress.Data.Async.Helpers;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class CustomBorder : Border
    {
        #region Constructors

        public CustomBorder()
        {
            this.Loaded += this.CustomGrid_Loaded;
        }

        #endregion Constructors

        #region Methods

        private void CustomGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var dc = (ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.DataContext;
                var m = dc.OriginalRow;
                this.DataContext = m;
            }
            catch (Exception ex) { }
        }

        #endregion Methods
    }
}
