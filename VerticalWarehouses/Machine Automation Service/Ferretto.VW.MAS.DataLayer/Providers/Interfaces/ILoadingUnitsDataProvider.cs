using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

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

        IEnumerable<LoadingUnit> GetAllTestUnits();

        IEnumerable<LoadingUnit> GetAllNotTestUnits();

        /// <summary>
        /// Gets the specified loading unit from the database.
        /// </summary>
        /// <param name="id">The id of the loading unit to retrieve.</param>
        /// <returns>The requested loading unit.</returns>
        /// <exception cref="EntityNotFoundException">An exception is thrown if no loading unit with the specified id exists in the database.</exception>
        LoadingUnit GetById(int id);

        IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics();

        IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics();

        void Import(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext context);

        void Insert(int loadingUnitsId);

        void Remove(int loadingUnitsId);

        void RemoveTestUnit(LoadingUnit loadingUnit);

        void Save(LoadingUnit loadingUnit);

        void SetHeight(int loadingUnitId, double height);

        void SetWeight(int loadingUnitId, double loadingUnitGrossWeight);

        void UpdateRange(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext dataContext);

        void UpdateWeightStatistics();

        #endregion
    }
}
