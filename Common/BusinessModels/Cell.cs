using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class Cell : BusinessObject<int>
    {
        #region Constructors

        public Cell(int id) : base(id)
        { }

        #endregion Constructors

        #region Properties

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClass { get; set; }

        [Display(Name = nameof(BusinessObjects.CellColumn), ResourceType = typeof(BusinessObjects))]
        public int? Column { get; set; }

        [Display(Name = nameof(BusinessObjects.CellFloor), ResourceType = typeof(BusinessObjects))]
        public int? Floor { get; set; }

        [Display(Name = nameof(BusinessObjects.CellNumber), ResourceType = typeof(BusinessObjects))]
        public int? Number { get; set; }

        [Display(Name = nameof(BusinessObjects.CellPriority), ResourceType = typeof(BusinessObjects))]
        public int Priority { get; set; }

        [Display(Name = nameof(BusinessObjects.CellSide), ResourceType = typeof(BusinessObjects))]
        public string Side { get; set; }

        [Display(Name = nameof(BusinessObjects.CellStatus), ResourceType = typeof(BusinessObjects))]
        public string Status { get; set; }

        [Display(Name = nameof(BusinessObjects.CellType), ResourceType = typeof(BusinessObjects))]
        public string Type { get; set; }

        [Display(Name = nameof(BusinessObjects.CellXCoordinate), ResourceType = typeof(BusinessObjects))]
        public int? XCoordinate { get; set; }

        [Display(Name = nameof(BusinessObjects.CellYCoordinate), ResourceType = typeof(BusinessObjects))]
        public int? YCoordinate { get; set; }

        [Display(Name = nameof(BusinessObjects.CellZCoordinate), ResourceType = typeof(BusinessObjects))]
        public int? ZCoordinate { get; set; }

        #endregion Properties
    }
}
