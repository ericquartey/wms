using Ferretto.VW.Simulator.Services.Models;

namespace Ferretto.VW.Simulator.Services.Interfaces
{
    public interface ICultureService
    {
        #region Properties

        ApplicationCulture ActiveCulture { get; }

        #endregion

        #region Methods

        void ApplyCulture(ApplicationCulture culture);

        #endregion
    }
}
