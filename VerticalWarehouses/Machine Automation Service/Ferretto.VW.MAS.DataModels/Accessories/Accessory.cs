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
    }
}
