using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager
{
    internal class BayTelescopicZeroConditionEvaluator : IBayTelescopicZeroConditionEvaluator
    {
        #region Fields

        private readonly IMachineResourcesProvider machineResourcesProvider;

        #endregion

        #region Constructors

        public BayTelescopicZeroConditionEvaluator(IMachineResourcesProvider machineResourcesProvider)
        {
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
        }

        #endregion

        #region Methods

        public bool IsSatisfied(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                case BayNumber.BayOne:
                    return this.machineResourcesProvider.TeleOkBay1;

                case BayNumber.BayTwo:
                    return this.machineResourcesProvider.TeleOkBay2;

                case BayNumber.BayThree:
                    return this.machineResourcesProvider.TeleOkBay3;

                default:
                    return false;
            }
        }

        #endregion
    }
}
