using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services
{
    public class ThemeChangedMessage
    {
        #region Constructors

        public ThemeChangedMessage(ApplicationTheme applicationTheme)
        {
            this.ApplicationTheme = applicationTheme;
        }

        #endregion

        #region Properties

        public ApplicationTheme ApplicationTheme { get; }

        #endregion
    }
}
