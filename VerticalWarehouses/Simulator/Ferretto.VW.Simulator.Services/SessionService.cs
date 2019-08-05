using System.Windows;
using Ferretto.VW.Simulator.Services.Interfaces;

#if DEBUG
#else
using System.Diagnostics;
#endif

namespace Ferretto.VW.Simulator.Services
{
    internal class SessionService : ISessionService
    {
        #region Methods

        public bool Shutdown()
        {
            try
            {
#if DEBUG
                Application.Current.Shutdown();
#else
                var processStartInfo = new ProcessStartInfo("shutdown", "/s /t 5");
                processStartInfo.CreateNoWindow = true;
                processStartInfo.UseShellExecute = false;
                Process.Start(processStartInfo);
#endif
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
