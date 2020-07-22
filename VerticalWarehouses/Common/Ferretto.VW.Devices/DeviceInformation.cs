using System;

namespace Ferretto.VW.Devices
{
    public class DeviceInformation
    {
        #region Properties

        public string FirmwareVersion { get; set; }

        public bool IsEmpty =>
            string.IsNullOrEmpty(this.SerialNumber)
            &&
            string.IsNullOrEmpty(this.ModelNumber)
            &&
            !this.ManufactureDate.HasValue
            &&
            string.IsNullOrEmpty(this.FirmwareVersion);

        public DateTime? ManufactureDate { get; set; }

        public string ModelNumber { get; set; }

        public string SerialNumber { get; set; }

        #endregion
    }
}
