using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class Cell : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.Aisle), ResourceType = typeof(BusinessObjects))]
        public string AisleName { get; set; }

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public string AreaName { get; set; }

        [Display(Name = nameof(BusinessObjects.Type), ResourceType = typeof(BusinessObjects))]
        public string CellTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CellColumn), ResourceType = typeof(BusinessObjects))]
        public int? Column { get; set; }

        [Display(Name = nameof(BusinessObjects.CellFloor), ResourceType = typeof(BusinessObjects))]
        public int? Floor { get; set; }

        [Display(Name = nameof(BusinessObjects.CellLoadingUnitsCount), ResourceType = typeof(BusinessObjects))]
        public int LoadingUnitsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.CellLoadingUnitsDescription), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitsDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CellNumber), ResourceType = typeof(BusinessObjects))]
        public int? Number { get; set; }

        [Display(Name = nameof(BusinessObjects.Priority), ResourceType = typeof(BusinessObjects))]
        public int Priority { get; set; }

        [Display(Name = nameof(BusinessObjects.CellSide), ResourceType = typeof(BusinessObjects))]
        public Side? Side { get; set; }

        [Display(Name = nameof(BusinessObjects.Status), ResourceType = typeof(BusinessObjects))]
        public string Status { get; set; }

        [Display(Name = nameof(BusinessObjects.CellXCoordinate), ResourceType = typeof(BusinessObjects))]
        public double? XCoordinate { get; set; }

        [Display(Name = nameof(BusinessObjects.CellYCoordinate), ResourceType = typeof(BusinessObjects))]
        public double? YCoordinate { get; set; }

        [Display(Name = nameof(BusinessObjects.CellZCoordinate), ResourceType = typeof(BusinessObjects))]
        public double? ZCoordinate { get; set; }

        #endregion
    }
}
