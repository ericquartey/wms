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

        public delegate void CurrentInstallationProcedureCompleteEvent();

        #endregion Delegates

        #region Events

        public static event BackToVWAppEvent BackToVWAppEventHandler;

        public static event CurrentInstallationProcedureCompleteEvent CurrentInstallationProcedureCompleteEventHandler;

        #endregion Events

        #region Methods

        public static void RaiseAndResetCurrentInstallationProcedureCompleteEvent()
        {
            if (CurrentInstallationProcedureCompleteEventHandler != null)
            {
                CurrentInstallationProcedureCompleteEventHandler();
                CurrentInstallationProcedureCompleteEventHandler = null;
            }
        }

        public static void RaiseBackToVWAppEvent()
        {
            if (BackToVWAppEventHandler != null)
            {
                BackToVWAppEventHandler();
            }
        }

        #endregion Methods
    }
}
