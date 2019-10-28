using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface ISensorsService
    {
        #region Properties

        bool IsLoadingUnitInBay { get; }

        bool IsLoadingUnitOnElevator { get; }

        #endregion

        #region Methods

        Task RefreshAsync();

        #endregion
    }
}
