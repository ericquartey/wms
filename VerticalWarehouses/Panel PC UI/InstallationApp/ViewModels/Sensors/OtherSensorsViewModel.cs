using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class OtherSensorsViewModel : BaseSensorsViewModel
    {
        #region Fields

        private bool securityFunctionActive;

        #endregion

        #region Constructors

        public OtherSensorsViewModel(ISensorsMachineService sensorsService)
            : base(sensorsService)
        {
        }

        #endregion

        #region Properties

        public bool SecurityFunctionActive
        {
            get => this.securityFunctionActive;
            set => this.SetProperty(ref this.securityFunctionActive, value);
        }

        #endregion
    }
}
