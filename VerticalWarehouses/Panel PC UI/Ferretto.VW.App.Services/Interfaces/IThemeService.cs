using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services
{
    public interface IThemeService
    {
        #region Properties

        ApplicationTheme ActiveTheme { get; }

        #endregion

        #region Methods

        void ApplyTheme(ApplicationTheme theme);

        #endregion
    }
}
