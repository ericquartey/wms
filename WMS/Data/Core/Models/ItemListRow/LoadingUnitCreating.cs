using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitCreating : BaseModel<int>
    {
        #region Fields

        private int? handlingParametersCorrection;

        private int height;

        private int weight;

        #endregion

        #region Properties

        public string AbcClassId { get; set; }

        public int? AisleId { get; set; }

        public int? AreaId { get; set; }

        public int? CellId { get; set; }

        public int? CellPositionId { get; set; }

        public string Code { get; set; }

        public int CompartmentsCount { get; set; }

        public int? HandlingParametersCorrection
        {
            get => this.handlingParametersCorrection;
            set => this.handlingParametersCorrection = CheckIfPositive(value);
        }

        public int Height
        {
            get => this.height;
            set => this.height = CheckIfStrictlyPositive(value);
        }

        public int InCycleCount { get; set; }

        public bool IsCellPairingFixed { get; set; }

        public string LoadingUnitStatusId { get; set; }

        public int LoadingUnitTypeId { get; set; }

        public string Note { get; set; }

        public int OtherCycleCount { get; set; }

        public int OutCycleCount { get; set; }

        public ReferenceType ReferenceType { get; set; }

        public int Weight
        {
            get => this.weight;
            set => this.weight = CheckIfPositive(value);
        }

        #endregion
    }
}
