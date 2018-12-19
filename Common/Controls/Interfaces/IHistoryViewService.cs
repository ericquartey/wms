using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface IHistoryViewService
    {
        #region Methods

        void Appear(string moduleName, string viewModelName, object data = null);

        void Previous();

        #endregion Methods
    }
}
