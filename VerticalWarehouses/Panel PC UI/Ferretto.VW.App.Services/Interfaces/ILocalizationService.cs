using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface ILocalizationService
    {
        #region Properties

        string ActualLanguageKey { get; }

        string AdminLanguageKey { get; }

        string InstallerLanguageKey { get; }

        string OperatorLanguageKey { get; }

        string ServiceLanguageKey { get; }

        #endregion

        #region Methods

        void ActivateCulture(UserAccessLevel userAccessLevel);

        void SetCulture(UserAccessLevel userAccessLevel, string culture);

        #endregion
    }
}
