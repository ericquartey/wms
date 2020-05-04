using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IInverterProgrammingProvider
    {
        #region Methods

        void Start(VertimagConfiguration vertimagConfiguration, BayNumber requestingBay, MessageActor sender);

        void Start(VertimagConfiguration vertimagConfiguration, byte inverterIndex, BayNumber requestingBay, MessageActor sender);

        void Stop(BayNumber requestingBay, MessageActor sender);

        #endregion
    }
}
