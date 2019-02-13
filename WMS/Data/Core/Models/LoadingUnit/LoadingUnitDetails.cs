using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitDetails : BaseModel<int>
    {
        #region Fields

        private int? handlingParametersCorrection;
        private int height;
        private int length;
        private int weight;
        private int width;

        #endregion

        #region Properties

        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        public int AisleId { get; set; }

        public int AreaId { get; set; }

        public int CellId { get; set; }

        public string CellPositionDescription { get; set; }

        public int CellPositionId { get; set; }

        public string Code { get; set; }

        public int CompartmentsCount { get; set; }

        public DateTime CreationDate { get; set; }

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

        public DateTime? InventoryDate { get; set; }

        public bool IsCellPairingFixed { get; set; }

        public DateTime? LastHandlingDate { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastStoreDate { get; set; }

        public int Length
        {
            get => this.length;
            set => this.length = CheckIfStrictlyPositive(value);
        }

        public string LoadingUnitStatusDescription { get; set; }

        public string LoadingUnitStatusId { get; set; }

        public string LoadingUnitTypeDescription { get; set; }

        public bool LoadingUnitTypeHasCompartments { get; set; }

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

        public int Width
        {
            get => this.width;
            set => this.width = CheckIfStrictlyPositive(value);
        }

        #endregion
    }
}
