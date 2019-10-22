using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IBaysProvider
    {
        #region Methods

        Bay Activate(BayNumber bayNumber);

        void AddElevatorPseudoBay();

        Bay AssignMissionOperation(BayNumber bayNumber, int? missionId, int? missionOperationId);

        double ConvertPulsesToMillimeters(double pulses, InverterIndex inverterIndex);

        Bay Deactivate(BayNumber bayNumber);

        IEnumerable<Bay> GetAll();

        BayNumber GetByAxis(IHomingMessageData data);

        BayNumber GetByInverterIndex(InverterIndex inverterIndex);

        BayNumber GetByIoIndex(IoIndex ioIndex, FieldMessageType messageType);

        Bay GetByIoIndex(IoIndex ioIndex);

        BayNumber GetByMovementType(IPositioningMessageData data);

        Bay GetByNumber(BayNumber bayNumber);

        double GetChainOffset(InverterIndex inverterIndex);

        InverterIndex GetInverterIndexByAxis(Axis axis, BayNumber bayNumber);

        InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayNumber bayNumber);

        InverterIndex GetInverterIndexByProfile(BayNumber bayNumber);

        IoIndex GetIoDevice(BayNumber bayNumber);

        double GetResolution(InverterIndex inverterIndex);

        Bay SetCurrentOperation(BayNumber bayNumber, BayOperation newOperation);

        Bay UpdatePosition(BayNumber bayNumber, int position, double height);

        #endregion
    }
}
