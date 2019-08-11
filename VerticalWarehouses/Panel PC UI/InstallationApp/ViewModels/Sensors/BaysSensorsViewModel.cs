using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BaysSensorsViewModel : BaseSensorsViewModel
    {
        #region Constructors

        public BaysSensorsViewModel(ISensorsMachineService sensorsMachineService)
            : base(sensorsMachineService)
        {
        }

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();
            this.SohwBack(true);
        }

        #endregion
    }
}
