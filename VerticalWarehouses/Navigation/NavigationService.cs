using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Navigation
{
    public static class NavigationService
    {
        #region Delegates

        public delegate void BackToVWAppEvent();

        #endregion Delegates

        #region Events

        public static event BackToVWAppEvent BackToVWAppEventHandler;

        #endregion Events

        #region Methods

        public static void RaiseBackToVWAppEvent()
        {
            BackToVWAppEventHandler();
        }

        #endregion Methods
    }
}
