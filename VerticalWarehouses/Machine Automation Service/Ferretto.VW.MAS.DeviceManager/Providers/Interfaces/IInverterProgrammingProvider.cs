using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IInverterProgrammingProvider
    {
        #region Methods

        IEnumerable<InverterParametersData> GetInvertersParametersData(IEnumerable<Inverter> inverters);

        void HardReset(Inverter inverter, BayNumber requestingBay, MessageActor sender);

        void Read(BayNumber requestingBay, MessageActor sender);

        void Read(InverterIndex inverterIndex, BayNumber requestingBay, MessageActor sender);

        void Reset(Inverter inverter, BayNumber requestingBay, MessageActor sender);

        void Start(Inverter inverter, BayNumber requestingBay, MessageActor sender);

        void Start(IEnumerable<Inverter> inverters, BayNumber requestingBay, MessageActor sender);

        #endregion
    }
}
