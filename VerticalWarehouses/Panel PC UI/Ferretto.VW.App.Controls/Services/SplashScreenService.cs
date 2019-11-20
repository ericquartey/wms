using System.Linq;
using System.Reflection;
using DevExpress.Xpf.Core;

namespace Ferretto.VW.App.Services
{
    public static class SplashScreenService
    {
        #region Properties

        public static string Copyright => (Assembly.GetEntryAssembly().GetCustomAttributes(false)
                .FirstOrDefault(attribute => attribute is AssemblyCopyrightAttribute) as AssemblyCopyrightAttribute)?.Copyright;

        public static string Version => Assembly.GetEntryAssembly().GetName()?.Version?.ToString();

        #endregion

        #region Methods

        public static void Hide()
        {
            if (DXSplashScreen.IsActive)
            {
                DXSplashScreen.Close();
            }
        }

        public static void SetMessage(string message)
        {
            if (DXSplashScreen.IsActive)
            {
                DXSplashScreen.SetState(message);
            }
        }

        public static void Show()
        {
            DXSplashScreen.Show<Controls.SplashScreen>();
        }

        #endregion
    }
}
