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

        public string IsConfigured { get; set; }

        public string IsEnabled { get; set; }

        #endregion

        // public DateTime? ManufactureDate {get; set;}

        // public string FirmwareVersion {get; set;}

        // public string Model {get; set;}

        // public string SerialNumber {get; set;}
    }
}
