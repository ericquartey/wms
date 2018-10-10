using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class LoadingUnit : BusinessObject<int>
    {
        #region Fields

        private int? cellColumn;
        private int? cellFloor;
        private int? cellNumber;

        #endregion Fields

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
            set => this.SetIfStrictlyPositive(ref this.cellColumn, value);
        }

        [Display(Name = nameof(BusinessObjects.CellFloor_extended), ResourceType = typeof(BusinessObjects))]
        public int? CellFloor
        {
            get => this.cellFloor;
            set => this.SetIfStrictlyPositive(ref this.cellFloor, value);
        }

        [Display(Name = nameof(BusinessObjects.CellNumber_extended), ResourceType = typeof(BusinessObjects))]
        public int? CellNumber
        {
            get => this.cellNumber;
            set => this.SetIfStrictlyPositive(ref this.cellNumber, value);
        }

        [Display(Name = nameof(BusinessObjects.CellPositionDescription), ResourceType = typeof(BusinessObjects))]
        public string CellPositionDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CellSide_extended), ResourceType = typeof(BusinessObjects))]
        public string CellSide { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        public int Id { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitStatus), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitTypeDescription), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitTypeDescription { get; set; }

        #endregion Properties
    }
}
