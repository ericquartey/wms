using System.Collections.Generic;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;

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
