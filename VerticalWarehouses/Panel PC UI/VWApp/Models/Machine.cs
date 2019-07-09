namespace Ferretto.VW.App.Models
{
    public class Machine : BaseModel
    {
        #region Fields

        private string model;

        private string serialNumber;

        #endregion

        #region Properties

        public string Model
        {
            get => this.model;
            set => this.SetProperty(ref this.model, value);
        }

        public string SerialNumber
        {
            get => this.serialNumber;
            set => this.SetProperty(ref this.serialNumber, value);
        }

        #endregion
    }
}
