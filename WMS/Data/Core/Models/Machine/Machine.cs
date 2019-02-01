using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace Ferretto.WMS.Data.Core.Models
{
    public class Machine : BaseModel<int>
    {
        #region Fields

        private int aisleId;

        private string aisleName;

        private string machineTypeDescription;

        private string machineTypeId;

        #endregion Fields

        #region Properties

        public int AisleId
        {
            get => this.aisleId;
            set
            {
                this.aisleId = value;
                this.ComputeAisleDescription();
            }
        }

        public string AisleName
        {
            get => this.aisleName;
            private set => this.aisleName = value;
        }

        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<Aisle> Aisles { get; set; }

        public string MachineTypeDescription
        {
            get => this.machineTypeDescription;
            private set => this.machineTypeDescription = value;
        }

        public string MachineTypeId
        {
            get => this.machineTypeId;
            set
            {
                this.machineTypeId = value;
                this.ComputeMachineTypeDescription();
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<MachineType> MachineTypes { get; set; }

        public string Model { get; set; }

        public long? MovedLoadingUnitsCount { get; set; }

        public string Nickname { get; set; }

        public string RegistrationNumber { get; set; }

        #endregion Properties

        #region Methods

        private void ComputeAisleDescription()
        {
            var aisle = this.Aisles?.SingleOrDefault(a => a.Id == this.AisleId);
            this.AisleName = $"{aisle?.AreaName} - {aisle?.Name}";
        }

        private void ComputeMachineTypeDescription()
        {
            this.MachineTypeDescription =
                this.MachineTypes?.SingleOrDefault(m => m.Id == this.MachineTypeId)?.Description;
        }

        #endregion Methods
    }
}
