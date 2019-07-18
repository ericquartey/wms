using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : ILoadingUnitStatistics
    {
        #region Methods

        public IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics()
        {
            var LoadingUnitSpaceStatistics = new List<LoadingUnitSpaceStatistics>()
            {
                 new LoadingUnitSpaceStatistics
                 {
                      CompartmentsFilled = 1,
                      Depth = 50,
                      TotalMissions = 10,
                      RatioFillingCompartments = 100,
                      TotalCompartments = 100,
                      Width = 3080
                 }
            };

            return LoadingUnitSpaceStatistics;
        }

        public IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics()
        {
            var loadingUnitWeightStatistics = new List<LoadingUnitWeightStatistics>()
            {
                 new LoadingUnitWeightStatistics
                 {
                      Height = 100,
                      MaxRatio =90,
                      MaxGrossWeight = 1200,
                      Tare = 100,
                      Code = "loading 1",
                      Weight = 500
                 }
            };

            return loadingUnitWeightStatistics;
        }

        #endregion
    }
}
