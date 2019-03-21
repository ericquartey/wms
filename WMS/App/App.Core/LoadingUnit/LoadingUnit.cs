using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class LoadingUnit : BusinessObject
    {
        #region Fields

        private int? cellColumn;

        private int? cellFloor;

        private int? cellNumber;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.Aisle), ResourceType = typeof(BusinessObjects))]
        public string AisleName { get; set; }

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public string AreaName { get; set; }

        [Display(Name = nameof(BusinessObjects.CellColumn_extended), ResourceType = typeof(BusinessObjects))]
        public int? CellColumn
        {
            get => this.cellColumn;
            set => this.SetProperty(ref this.cellColumn, value);
        }

        [Display(Name = nameof(BusinessObjects.CellFloor_extended), ResourceType = typeof(BusinessObjects))]
        public int? CellFloor
        {
            get => this.cellFloor;
            set => this.SetProperty(ref this.cellFloor, value);
        }

        [Display(Name = nameof(BusinessObjects.CellNumber_extended), ResourceType = typeof(BusinessObjects))]
        public int? CellNumber
        {
            get => this.cellNumber;
            set => this.SetProperty(ref this.cellNumber, value);
        }

        [Display(Name = nameof(BusinessObjects.CellPositionDescription), ResourceType = typeof(BusinessObjects))]
        public string CellPositionDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CellSide_extended), ResourceType = typeof(BusinessObjects))]
        public Side? CellSide { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitStatus), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitTypeDescription), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitTypeDescription { get; set; }

        #endregion
    }
}
