using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    internal class WeightCheckMissionProvider : BaseProvider, IWeightCheckMissionProvider
    {
        #region Fields

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        #endregion

        #region Constructors

        public WeightCheckMissionProvider(
            ILoadingUnitsProvider loadingUnitsProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            if (loadingUnitsProvider is null)
            {
                throw new ArgumentNullException(nameof(loadingUnitsProvider));
            }

            this.loadingUnitsProvider = loadingUnitsProvider;
        }

        #endregion

        #region Methods

        public void Start(int loadingUnitId, double runToTest, double weight)
        {
            if (runToTest < 0)
            {
                throw new ArgumentOutOfRangeException(
                    $"LoadingUnit {loadingUnitId}, runToTest must be positive."); // TODO localize string
            }

            if (weight <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    $"LoadingUnit {loadingUnitId}, weight must be greater than zero."); // TODO localize string
            }

            var loadingUnit = this.loadingUnitsProvider.GetById(loadingUnitId);

            if (loadingUnit.MaxNetWeight < loadingUnit.Tare + (decimal)weight)
            {
                throw new ArgumentOutOfRangeException(// TODO localize string
                    $"LoadingUnit {loadingUnitId}, weight ({weight}) must be less than ({loadingUnit.MaxNetWeight - loadingUnit.Tare}).");
            }

            // TODO execute operations
            Task.Delay(5000);
        }

        public void Stop()
        {
            // TODO execute stop command
        }

        #endregion
    }
}
