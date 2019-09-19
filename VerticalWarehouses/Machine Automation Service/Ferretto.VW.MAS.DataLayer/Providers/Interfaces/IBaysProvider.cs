using System.Collections.Generic;
using System.Net;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IBaysProvider
    {


        #region Properties

        int BayInverterPosition { get; }

        int ShutterInverterPosition { get; }

        #endregion



        #region Methods

        Bay Activate(BayNumber bayIndex);

        Bay AssignMissionOperation(BayNumber bayNumber, int? missionId, int? missionOperationId);

        void Create(Bay bay);

        Bay Deactivate(BayNumber bayIndex);

        IEnumerable<Bay> GetAll();

        Bay GetByIndex(BayNumber bayIndex);

        BayNumber GetByInverterIndex(InverterIndex inverterIndex);

        BayNumber GetByIoIndex(IoIndex ioIndex, FieldMessageType messageType);

        Bay GetByIpAddress(IPAddress remoteIpAddress);

        BayNumber GetByMovementType(IPositioningMessageData data);

        InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayNumber bayIndex);

        List<InverterIndex> GetInverterList(BayNumber bayIndex);

        IoIndex GetIoDevice(BayNumber bayIndex);

        Bay SetCurrentOperation(BayNumber bayIndex, BayOperation newOperation);

        void Update(BayNumber bayIndex, string ipAddress, BayType bayType);

        Bay UpdatePosition(BayNumber bayIndex, int position, decimal height);

        #endregion
    }
}
