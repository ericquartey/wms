using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Models;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class VerticalOriginSetupStatusProvider : IVerticalOriginSetupStatusProvider
    {
        #region Fields

        private readonly SetupStepStatus setupStatus = new SetupStepStatus { CanBePerformed = true };

        #endregion

        #region Methods

        public void Complete()
        {
            this.setupStatus.IsCompleted = true;
        }

        public SetupStepStatus Get()
        {
            return this.setupStatus;
        }

        #endregion
    }
}
