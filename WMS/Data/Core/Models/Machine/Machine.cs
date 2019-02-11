namespace Ferretto.WMS.Data.Core.Models
{
    public class Machine : BaseModel<int>
    {
        #region Fields

        #endregion

        #region Properties

        public int AisleId { get; set; }

        public string AisleName { get; private set; }

        public string MachineTypeDescription { get; private set; }

        public string MachineTypeId { get; set; }

        public string Model { get; set; }

        public long? MovedLoadingUnitsCount { get; set; }

        public string Nickname { get; set; }

        public string RegistrationNumber { get; set; }

        #endregion
    }
}
