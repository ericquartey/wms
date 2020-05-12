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

        public bool IsConfigured { get; set; }

        public bool IsEnabled { get; set; }

        #endregion
    }
}
