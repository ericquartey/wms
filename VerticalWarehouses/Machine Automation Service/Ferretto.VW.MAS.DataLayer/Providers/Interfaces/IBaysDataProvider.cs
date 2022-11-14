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

        void AddElevatorPseudoBay();

        Bay AssignMission(BayNumber bayNumber, Mission mission);

        void CheckBayFindZeroLimit();

        /// <summary>
        /// Sends a CheckIntrusion command to Device Manager
        /// </summary>
        /// <param name="bayNumber"></param>
        /// <param name="enable"></param>
        /// <returns>true if CheckIntrusion is enabled for this bay number</returns>
        bool CheckIntrusion(BayNumber bayNumber, bool enable);

        void CheckProfileConst();

        Bay ClearMission(BayNumber bayNumber);

        double ConvertProfileToHeight(ushort profile, int positionId);

        void FindZero(BayNumber bayNumber);

        IEnumerable<Bay> GetAll();

        IEnumerable<BayNumber> GetBayNumbers();

        WarehouseSide GetBaySide(BayNumber bayNumber);

        BayNumber GetByAxis(IHomingMessageData data);

        Bay GetByBayPositionId(int id);

        Bay GetByCell(Cell cell);

        /// <summary>
        /// Gets the bay identified by the given identifier.
        /// </summary>
        /// <param name="id">The identifier of the bay to retrieve.</param>
        /// <returns>The bay identified by the given identifier, or null if no bay with the given identifier exists.</returns>
        Bay GetByIdOrDefault(int id);

        BayNumber GetByInverterIndex(InverterIndex inverterIndex, FieldMessageType messageType = FieldMessageType.NoType);

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

        Bay GetByNumberCarousel(BayNumber bayNumber);

        Bay GetByNumberExternal(BayNumber bayNumber);

        Bay GetByNumberNoInclude(BayNumber bayNumber);

        Bay GetByNumberPositions(BayNumber bayNumber);

        Bay GetByNumberShutter(BayNumber bayNumber);

        int GetCarouselBayFindZeroLimit(BayNumber bayNumber);

        double GetChainOffset(InverterIndex inverterIndex);

        double GetChainPosition(BayNumber bayNumber);

        InverterIndex GetInverterIndexByAxis(Axis axis, BayNumber bayNumber);

        InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayNumber bayNumber);

        InverterIndex GetInverterIndexByProfile(BayNumber bayNumber);

        IoIndex GetIoDevice(BayNumber bayNumber);

        bool GetIsExternal(BayNumber bayNumber);

        bool GetIsTelescopic(BayNumber bayNumber);

        bool GetLightOn(BayNumber bayNumber);

        LoadingUnit GetLoadingUnitByDestination(LoadingUnitLocation location);

        double? GetLoadingUnitDestinationHeight(LoadingUnitLocation location);

        LoadingUnitLocation GetLoadingUnitLocationByLoadingUnit(int loadingUnitId);

        /// <summary>
        /// Gets the bay containing the specified bay position.
        /// </summary>
        /// <param name="bayPositionId">The id of the bay position contained in the bay to retrieve.</param>
        /// <returns>The bay containing the specified bay position.</returns>
        BayPosition GetPositionById(int bayPositionId);

        BayPosition GetPositionByLocation(LoadingUnitLocation destination);

        double GetResolution(InverterIndex inverterIndex);

        InverterIndex GetShutterInverterIndex(BayNumber bayNumber);

        void IncrementCycles(BayNumber bayNumber);

        bool IsLoadUnitInBay(BayNumber bayNumber, int id);

        bool IsMissionInBay(Mission mission);

        void Light(BayNumber bayNumber, bool enable);

        void NotifyRemoveLoadUnit(int loadingUnitId, LoadingUnitLocation location);

        void PerformHoming(BayNumber bayNumber);

        void RemoveLoadingUnit(int loadingUnitId);

        void ResetMachine();

        void SetAllOperationsBay(bool pick, bool put, bool view, bool inventory, bool barcodeAutomaticPut, int bayid, bool showBarcodeImage, bool checkListContinueInOtherMachine);

        Bay SetBayActive(BayNumber bayNumber, bool active);

        void SetChainPosition(BayNumber bayNumber, double value);

        Bay SetCurrentOperation(BayNumber bayNumber, BayOperation newOperation);

        /// <summary>
        /// Specifies that the given loading unit is now located in a bay position.
        /// It updates bay and LoadUnit db tables.
        /// </summary>
        /// <param name="bayPositionId">The identifier of the bay position where the loading unit is now located.</param>
        /// <param name="loadingUnitId">The identifier of the loading unit.</param>
        /// <param name="height">"only when it is not null"</param>
        void SetLoadingUnit(int bayPositionId, int? loadingUnitId, double? height = null);

        void SetProfileConstBay(BayNumber bayNumber, double k0, double k1);

        void SetRotationClass(BayNumber bayNumber);

        void UpdateELevatorDistance(BayNumber bayNumber, double distance);

        void UpdateExtraRace(BayNumber bayNumber, double extraRace);

        void UpdateLastCalibrationCycles(BayNumber bayNumber);

        void UpdateLastIdealPosition(double position, BayNumber bayNumber);

        Bay UpdatePosition(BayNumber bayNumber, int position, double height);

        void UpdateRace(BayNumber bayNumber, double race);

        void UpdateResolution(BayNumber bayNumber, double newRace);

        #endregion
    }
}
