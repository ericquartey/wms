using System.Threading.Tasks;
using Ferretto.VW.Utils.Source;

namespace BackgroundService
{
    public interface ISensorsStatesHub
    {
        #region Methods

        Task OnSensorsChanged(SensorsStates sensors);

        #endregion
    }
}
