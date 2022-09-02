using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ILoadingUnitsDataProvider
    {
        #region Methods

        void Add(IEnumerable<LoadingUnit> loadingUnits);

        void AddTestUnit(LoadingUnit loadingUnit);

        MachineErrorCode CheckWeight(int id);

        int CountIntoMachine();

        IEnumerable<LoadingUnit> GetAll();

        IEnumerable<LoadingUnit> GetAllCompacting();

        IEnumerable<LoadingUnit> GetAllNotTestUnits();

        IEnumerable<LoadingUnit> GetAllTestUnits();

        /// <summary>
        /// Gets the specified loading unit from the database.
        /// </summary>
        /// <param name="id">The id of the loading unit to retrieve.</param>
        /// <returns>The requested loading unit.</returns>
        /// <exception cref="EntityNotFoundException">An exception is thrown if no loading unit with the specified id exists in the database.</exception>
        LoadingUnit GetById(int id);

        LoadingUnit GetCellById(int id);

        double GetLoadUnitMaxHeight();

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        void Import(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext context);

        void Insert(int loadingUnitsId);

        void Remove(int loadingUnitsId);

        void RemoveTestUnit(LoadingUnit loadingUnit);

        Task SaveAsync(LoadingUnit loadingUnit);

        Task SaveToWmsAsync(int loadingUnitsId);

        void SetHeight(int loadingUnitId, double height);

        void SetLaserOffset(int id, double laserOffset);

        void SetMissionCountRotation(int id, MissionType missionType);

        bool SetRotationClass();

        void SetRotationClassFromUI(int id, string rotationClass);

        void SetStatus(int id, LoadingUnitStatus status);

        /// <summary>
        /// </summary>
        /// <param name="loadingUnitId"></param>
        /// <param name="loadingUnitGrossWeight">Tare + ElevatorWeight + NetWeight</param>
        void SetWeight(int loadingUnitId, double loadingUnitGrossWeight);

        /// <summary>
        /// </summary>
        /// <param name="loadingUnitId"></param>
        /// <param name="loadingUnitGrossWeight">Tare + NetWeight</param>
        void SetWeightFromUI(int loadingUnitId, double loadingUnitGrossWeight);

        void TryAdd(int loadingUnitId);

        void UpdateRange(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext dataContext);

        void UpdateWeightStatistics();

        #endregion
    }
}
