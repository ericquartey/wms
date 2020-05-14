using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class Accessory : DataModel
    {
        #region Constructors

        protected Accessory()
        {
        }

        #endregion

        #region Properties

        public DeviceInformation DeviceInformation { get; set; }

        [Obsolete("Use the IsConfiguredNew field instead.")]
        public string IsConfigured { get; set; } = "Obsolete";

        public bool IsConfiguredNew { get; set; }

        [Obsolete("Use the IsEnabledNew field instead.")]
        public string IsEnabled { get; set; } = "Obsolete";

        public bool IsEnabledNew { get; set; }

        #endregion
    }
}
