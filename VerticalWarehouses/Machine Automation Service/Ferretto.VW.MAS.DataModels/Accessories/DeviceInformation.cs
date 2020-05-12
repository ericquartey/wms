using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class DeviceInformation : DataModel
    {
        #region Properties

        public string FirmwareVersion { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public string ModelNumber { get; set; }

        public string SerialNumber { get; set; }

        #endregion
    }
}
