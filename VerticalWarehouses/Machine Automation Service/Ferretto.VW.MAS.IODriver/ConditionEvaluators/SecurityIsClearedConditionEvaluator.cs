using System;
using System.Linq;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.IODriver
{
    internal class SecurityIsClearedConditionEvaluator : ISecurityIsClearedConditionEvaluator
    {
        #region Fields

        private readonly IIoDevicesProvider ioDevicesProvider;

        #endregion

        #region Constructors

        public SecurityIsClearedConditionEvaluator(IIoDevicesProvider ioDevicesProvider)
        {
            this.ioDevicesProvider = ioDevicesProvider ?? throw new ArgumentNullException(nameof(ioDevicesProvider));
        }

        #endregion

        #region Methods

        public bool IsSatisfied()
        {
            var mainDevice = this.ioDevicesProvider.Devices.SingleOrDefault(d => d.IoIndex == IoIndex.IoDevice1);
            if (mainDevice is null)
            {
                return false;
            }

            return !mainDevice.ResetSecurity;
        }

        #endregion
    }
}
