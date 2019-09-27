using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IBaysProvider
    {
        #region Methods

        Bay Activate(BayNumber bayIndex);

        void AddElevatorPseudoBay();

        Bay AssignMissionOperation(BayNumber bayNumber, int? missionId, int? missionOperationId);

        void Create(Bay bay);

        Bay Deactivate(BayNumber bayIndex);

        IEnumerable<Bay> GetAll();

        BayNumber GetByInverterIndex(InverterIndex inverterIndex);

        BayNumber GetByIoIndex(IoIndex ioIndex, FieldMessageType messageType);

        BayNumber GetByMovementType(IPositioningMessageData data);

        Bay GetByNumber(BayNumber bayIndex);

        InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayNumber bayIndex);

        IoIndex GetIoDevice(BayNumber bayIndex);

        Bay SetCurrentOperation(BayNumber bayIndex, BayOperation newOperation);

        Bay UpdatePosition(BayNumber bayIndex, int position, double height);

        #endregion
    }
}
