using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IBayManager
    {
        #region Properties

        double ChainPosition { get; }

        MachineIdentity Identity { get; }

        #endregion

        #region Methods

        Task<LoadingUnit> GetAccessibleLoadingUnitAsync();

        Task<BayAccessories> GetBayAccessoriesAsync();

        /// <exception cref="MasWebApiException"></exception>
        Task<Bay> GetBayAsync();

        /// <exception cref="MasWebApiException"></exception>
        Task InitializeAsync();

        /// <exception cref="MasWebApiException"></exception>
        Task SetAlphaNumericBarAsync(bool isEnabled, IPAddress ipAddress, int port);

        /// <exception cref="MasWebApiException"></exception>
        Task SetSetLaserPointerAsync(bool isEnabled, IPAddress ipAddress, int port, double xOffset, double yOffset, double zOffsetLowerPosition, double zOffsetUpperPosition);

        #endregion
    }
}
