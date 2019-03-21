using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class CellDetails : BusinessObject
    {
        #region Fields

        private string abcClassId;

        private int aisleId;

        private int areaId;

        private int cellStatusId;

        private int? cellTypeId;

        private int? column;

        private int? floor;

        private int? number;

        private int priority;

        private Side side;

        private double? xCoordinate;

        private double? yCoordinate;

        private double? zCoordinate;

        #endregion

        #region Properties

        public IEnumerable<EnumerationString> AbcClassChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassId
        {
            get => this.abcClassId;
            set => this.SetProperty(ref this.abcClassId, value);
        }

        public IEnumerable<Enumeration> AisleChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.Aisle), ResourceType = typeof(BusinessObjects))]
        public int AisleId
        {
            get => this.aisleId;
            set => this.SetProperty(ref this.aisleId, value);
        }

        public int AreaId
        {
            get => this.areaId;
            set => this.SetProperty(ref this.areaId, value);
        }

        public IEnumerable<Enumeration> CellStatusChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.CellStatus), ResourceType = typeof(BusinessObjects))]
        public int CellStatusId
        {
            get => this.cellStatusId;
            set => this.SetProperty(ref this.cellStatusId, value);
        }

        public IEnumerable<Enumeration> CellTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.CellType), ResourceType = typeof(BusinessObjects))]
        public int? CellTypeId
        {
            get => this.cellTypeId;
            set => this.SetProperty(ref this.cellTypeId, value);
        }

        [Display(Name = nameof(BusinessObjects.CellColumn), ResourceType = typeof(BusinessObjects))]
        public int? Column
        {
            get => this.column;
            set => this.SetIfStrictlyPositive(ref this.column, value);
        }

        [Display(Name = nameof(BusinessObjects.CellFloor), ResourceType = typeof(BusinessObjects))]
        public int? Floor
        {
            get => this.floor;
            set => this.SetIfStrictlyPositive(ref this.floor, value);
        }

        public IEnumerable<LoadingUnitDetails> LoadingUnits { get; set; }

        public int LoadingUnitsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.CellNumber), ResourceType = typeof(BusinessObjects))]
        public int? Number
        {
            get => this.number;
            set => this.SetIfStrictlyPositive(ref this.number, value);
        }

        [Display(Name = nameof(BusinessObjects.CellPriority), ResourceType = typeof(BusinessObjects))]
        public int Priority
        {
            get => this.priority;
            set => this.SetIfStrictlyPositive(ref this.priority, value);
        }

        [Display(Name = nameof(BusinessObjects.CellSide), ResourceType = typeof(BusinessObjects))]
        public Side Side
        {
            get => this.side;
            set => this.SetProperty(ref this.side, value);
        }

        public IEnumerable<Enumeration> SideChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.CellXCoordinate), ResourceType = typeof(BusinessObjects))]
        public double? XCoordinate
        {
            get => this.xCoordinate;
            set => this.SetProperty(ref this.xCoordinate, value);
        }

        [Display(Name = nameof(BusinessObjects.CellYCoordinate), ResourceType = typeof(BusinessObjects))]
        public double? YCoordinate
        {
            get => this.yCoordinate;
            set => this.SetProperty(ref this.yCoordinate, value);
        }

        [Display(Name = nameof(BusinessObjects.CellZCoordinate), ResourceType = typeof(BusinessObjects))]
        public double? ZCoordinate
        {
            get => this.zCoordinate;
            set => this.SetProperty(ref this.zCoordinate, value);
        }

        #endregion
    }
}
