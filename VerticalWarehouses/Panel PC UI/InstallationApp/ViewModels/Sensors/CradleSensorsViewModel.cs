using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class CradleSensorsViewModel : BaseSensorsViewModel
    {
        #region Fields

        private bool isZeroPawlSensorActive;

        private bool luPresentInMachineSide;

        private bool luPresentInOperatorSide;

        #endregion

        #region Constructors

        public CradleSensorsViewModel(ISensorsMachineService sensorsMachineService)
            : base(sensorsMachineService)
        {
        }

        #endregion

        #region Properties

        public bool IsZeroPawlSensorActive
        {
            get => this.isZeroPawlSensorActive;
            set => this.SetProperty(ref this.isZeroPawlSensorActive, value);
        }

        public bool LuPresentInMachineSide
        {
            get => this.luPresentInMachineSide;
            set => this.SetProperty(ref this.luPresentInMachineSide, value);
        }

        public bool LuPresentInOperatorSide
        {
            get => this.luPresentInOperatorSide;
            set => this.SetProperty(ref this.luPresentInOperatorSide, value);
        }

        #endregion
    }
}
