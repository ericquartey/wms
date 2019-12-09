using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IBaysDataProvider
    {
        #region Methods

        Bay Activate(BayNumber bayNumber);

        void AddElevatorPseudoBay();

        Bay AssignMission(BayNumber bayNumber, Mission mission);

        Bay AssignWmsMission(BayNumber bayNumber, Mission mission, int? wmsMissionOperationId);

        Bay ClearMission(BayNumber bayNumber);

        double ConvertProfileToHeight(ushort profile);

        double ConvertPulsesToMillimeters(double pulses, InverterIndex inverterIndex);

        Bay Deactivate(BayNumber bayNumber);

        void FindZero(BayNumber bayNumber);

        IEnumerable<Bay> GetAll();

        CarouselManualParameters GetAssistedMovementsCarousel(BayNumber bayNumber);

        ShutterManualParameters GetAssistedMovementsShutter(BayNumber bayNumber);

        BayNumber GetByAxis(IHomingMessageData data);

        Bay GetByBayPositionId(int id);

        /// <summary>
        /// Gets the bay identified by the given identifier.
        /// </summary>
        /// <param name="id">The identifier of the bay to retrieve.</param>
        /// <returns>The bay identified by the given identifier, or null if no bay with the given identifier exists.</returns>
        Bay GetByIdOrDefault(int id);

        BayNumber GetByInverterIndex(InverterIndex inverterIndex);

        BayNumber GetByIoIndex(IoIndex ioIndex, FieldMessageType messageType);

        Bay GetByIoIndex(IoIndex ioIndex);

        Bay GetByLoadingUnitLocation(LoadingUnitLocation location);

        BayNumber GetByMovementType(IPositioningMessageData data);

        /// <summary>
        /// Gets the bay identified by the given number.
        /// </summary>
        /// <param name="bayNumber">The number of the bay to retrieve.</param>
        /// <returns>The bay identified by the given number.</returns>
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

        CarouselManualParameters GetManualMovementsCarousel(BayNumber bayNumber);

        ShutterManualParameters GetManualMovementsShutter(BayNumber bayNumber);

        /// <summary>
        /// Gets the bay containing the specified bay position.
        /// </summary>
        /// <param name="bayPositionId">The id of the bay position contained in the bay to retrieve.</param>
        /// <returns>The bay containing the specified bay position.</returns>
        BayPosition GetPositionById(int bayPositionId);

        BayPosition GetPositionByLocation(LoadingUnitLocation destination);

        double GetResolution(InverterIndex inverterIndex);

        void PerformHoming(BayNumber bayNumber);

        void RemoveLoadingUnit(int loadingUnitId);

        void ResetMachine();

        void SetChainPosition(BayNumber bayNumber, double value);

        Bay SetCurrentOperation(BayNumber bayNumber, BayOperation newOperation);

        /// <summary>
        /// Specifies that the given loading unit is now located in a bay position.
        /// </summary>
        /// <param name="bayPositionId">The identifier of the bay position where the loading unit is now located.</param>
        /// <param name="loadingUnitId">The identifier of the loading unit.</param>
        void SetLoadingUnit(int bayPositionId, int? loadingUnitId);

        void UpdateLastIdealPosition(double position, BayNumber bayNumber);

        Bay UpdatePosition(BayNumber bayNumber, int position, double height);

        #endregion
    }
}
