using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager
{
    internal class FireAlarmConditionEvaluator : IFireAlarmConditionEvaluator
    {
        #region Fields

        private readonly IMachineResourcesProvider machineResourcesProvider;

        #endregion

        #region Constructors

        public FireAlarmConditionEvaluator(IMachineResourcesProvider machineResourcesProvider)
        {
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
        }

        #endregion

        #region Methods

        public bool IsSatisfied(BayNumber bayNumber)
        {
            return !this.machineResourcesProvider.FireAlarm;
        }

        #endregion
    }
}
