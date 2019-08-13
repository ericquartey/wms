using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterSensorsViewModel : BaseSensorsViewModel
    {
        #region Fields

        private bool gateSensorA;

        private bool gateSensorB;

        #endregion

        #region Constructors

        public ShutterSensorsViewModel(IMachineSensorsService sensorsService)
            : base(sensorsService)
        {
        }

        #endregion

        #region Properties

        public bool GateSensorA
        {
            get => this.gateSensorA;
            set => this.SetProperty(ref this.gateSensorA, value);
        }

        public bool GateSensorB
        {
            get => this.gateSensorB;
            set => this.SetProperty(ref this.gateSensorB, value);
        }

        #endregion
    }
}
