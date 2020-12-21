using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IInverterProgrammingProvider
    {
        #region Methods

        IEnumerable<InverterParametersData> GetInvertersParametersData(IEnumerable<Inverter> inverters);

        void Read(IEnumerable<Inverter> inverters, BayNumber requestingBay, MessageActor sender);

        void Read(Inverter inverter, BayNumber requestingBay, MessageActor sender);

        void Start(Inverter inverter, BayNumber requestingBay, MessageActor sender);

        void Start(IEnumerable<Inverter> inverters, BayNumber requestingBay, MessageActor sender);

        void Stop(BayNumber requestingBay, MessageActor sender);

        #endregion
    }
}
