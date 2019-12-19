using System;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager
{
    internal class ElevatorHorizontalChainZeroConditionEvaluator : IElevatorHorizontalChainZeroConditionEvaluator
    {
        #region Fields

        private readonly IMachineResourcesProvider machineResourcesProvider;

        #endregion

        #region Constructors

        public ElevatorHorizontalChainZeroConditionEvaluator(IMachineResourcesProvider machineResourcesProvider)
        {
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
        }

        #endregion

        #region Methods

        public bool IsSatisfied()
        {
            return this.machineResourcesProvider.IsSensorZeroOnCradle || this.machineResourcesProvider.IsDrawerCompletelyOnCradle;
        }

        #endregion
    }
}
