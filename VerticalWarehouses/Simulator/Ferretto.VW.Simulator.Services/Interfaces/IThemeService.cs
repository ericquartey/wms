using Ferretto.VW.Simulator.Services.Models;

namespace Ferretto.VW.Simulator.Services.Interfaces
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
