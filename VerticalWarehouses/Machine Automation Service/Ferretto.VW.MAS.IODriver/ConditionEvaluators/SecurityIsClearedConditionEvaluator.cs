using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver
{
    internal sealed class SecurityIsClearedConditionEvaluator : ISecurityIsClearedConditionEvaluator
    {
        #region Fields

        private readonly IIoDevicesProvider ioDevicesProvider;

        private readonly IMachineProvider machineProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public SecurityIsClearedConditionEvaluator(
            ILogger logger,
            IIoDevicesProvider ioDevicesProvider,
            IMachineProvider machineProvider)
        {
            this.ioDevicesProvider = ioDevicesProvider ?? throw new ArgumentNullException(nameof(ioDevicesProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public bool IsSatisfied(BayNumber bayNumber)
        {
            try
            {
                var mainDevice = this.ioDevicesProvider.Devices.SingleOrDefault(d => d.IoIndex == IoIndex.IoDevice1);
                if (mainDevice is null)
                {
                    return false;
                }

                var fireAlarm = this.machineProvider.IsFireAlarmActive() ? !mainDevice.PreFireAlarm && !mainDevice.FireAlarm : true;

                var sensitiveEdgeAlarm = this.machineProvider.IsSpeaActive() && !this.machineProvider.IsSensitiveCarpetsBypass() ? mainDevice.SensitiveEdgeAlarm : false;
                var sensitiveCarpetsAlarm = this.machineProvider.IsSpeaActive() && !this.machineProvider.IsSensitiveCarpetsBypass() ? mainDevice.SensitiveCarpetsAlarm : false;

                var heightAlarm = this.machineProvider.IsHeightAlarmActive();

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
                    fireAlarm
                    &&
                    !sensitiveEdgeAlarm
                    &&
                    !sensitiveCarpetsAlarm
                    &&
                    !heightAlarm;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Test1 ResetSecurity " + ex.Message);
                return false;
            }
        }

        #endregion
    }
}
