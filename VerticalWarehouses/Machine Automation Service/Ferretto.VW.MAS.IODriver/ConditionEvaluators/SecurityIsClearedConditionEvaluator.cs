using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.IODriver
{
    internal sealed class SecurityIsClearedConditionEvaluator : ISecurityIsClearedConditionEvaluator
    {
        #region Fields

        private readonly IIoDevicesProvider ioDevicesProvider;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public SecurityIsClearedConditionEvaluator(
            IIoDevicesProvider ioDevicesProvider,
            IMachineProvider machineProvider)
        {
            this.ioDevicesProvider = ioDevicesProvider ?? throw new ArgumentNullException(nameof(ioDevicesProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
        }

        #endregion

        #region Methods

        public bool IsSatisfied(BayNumber bayNumber)
        {
            var mainDevice = this.ioDevicesProvider.Devices.SingleOrDefault(d => d.IoIndex == IoIndex.IoDevice1);
            if (mainDevice is null)
            {
                return false;
            }

            var fireAlarm = this.machineProvider.IsFireAlarmActive() ? !mainDevice.PreFireAlarm && !mainDevice.FireAlarm : true;

            return !mainDevice.ResetSecurity
                &&
                !mainDevice.MushroomEmergency
                &&
                !mainDevice.MicroCarterLeftSideBay
                &&
                !mainDevice.AntiIntrusionShutterBay
                &&
                !mainDevice.MicroCarterRightSideBay
                &&
                fireAlarm;
        }

        #endregion
    }
}
