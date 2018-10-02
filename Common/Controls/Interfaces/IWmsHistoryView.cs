using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface IWmsHistoryView
    {
        // void Appear(INavigableView view);

        #region Methods

        void Appear(string viewModelName);

        #endregion Methods
    }
}
