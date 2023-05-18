using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager
{
    internal class HeightAlarmConditionEvaluator : IHeightAlarmConditionEvaluator
    {
        #region Fields

        private readonly IMachineProvider machineProvider;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        #endregion

        #region Constructors

        public HeightAlarmConditionEvaluator(IMachineResourcesProvider machineResourcesProvider,
            IMachineProvider machineProvider)
        {
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
        }

        #endregion

        #region Methods

        public bool IsSatisfied(BayNumber bayNumber)
        {
            //this.machineProvider.SetHeightAlarm(!this.machineResourcesProvider.HeightAlarm);
            return !this.machineResourcesProvider.HeightAlarm;
        }

        #endregion
    }
}
