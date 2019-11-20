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

        double ConvertProfileToHeight(ushort profile);

        double ConvertPulsesToMillimeters(double pulses, InverterIndex inverterIndex);

        Bay Deactivate(BayNumber bayNumber);

        IEnumerable<Bay> GetAll();

        BayNumber GetByAxis(IHomingMessageData data);

        Bay GetByBayPositionId(int id);

        BayNumber GetByInverterIndex(InverterIndex inverterIndex);

        BayNumber GetByIoIndex(IoIndex ioIndex, FieldMessageType messageType);

        Bay GetByIoIndex(IoIndex ioIndex);

        Bay GetByLoadingUnitLocation(LoadingUnitLocation location);

        BayNumber GetByMovementType(IPositioningMessageData data);

        Bay GetByNumber(BayNumber bayNumber);

        double GetChainOffset(InverterIndex inverterIndex);

        double GetChainPosition(BayNumber bayNumber);

        InverterIndex GetInverterIndexByAxis(Axis axis, BayNumber bayNumber);

        InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayNumber bayNumber);

        InverterIndex GetInverterIndexByProfile(BayNumber bayNumber);

        IoIndex GetIoDevice(BayNumber bayNumber);

        LoadingUnit GetLoadingUnitByDestination(LoadingUnitLocation location);

        double? GetLoadingUnitDestinationHeight(LoadingUnitLocation location);

        LoadingUnitLocation GetLoadingUnitLocationByLoadingUnit(int loadingUnitId);

        BayPosition GetPositionById(int bayPositionId);

        BayPosition GetPositionByLocation(LoadingUnitLocation destination);

        double GetResolution(InverterIndex inverterIndex);

        void ResetMachine();

        void SetChainPosition(BayNumber bayNumber, double value);

        Bay SetCurrentOperation(BayNumber bayNumber, BayOperation newOperation);

        void SetLoadingUnit(int bayPositionId, int? loadingUnitId);

        Bay UpdatePosition(BayNumber bayNumber, int position, double height);

        void UpdateRealTimePosition(Bay bay, double position);

        #endregion
    }
}
