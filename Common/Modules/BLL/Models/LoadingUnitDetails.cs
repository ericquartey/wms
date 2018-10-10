using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.Common.Modules.BLL.Models
{
    public sealed class LoadingUnitDetails : BusinessObject<int>
    {
        #region Fields

        private int length;
        private int width;

        #endregion Fields

        #region Properties

        public IEnumerable<Enumeration<string>> AbcClassChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassId { get; set; }

        public IEnumerable<Enumeration<int>> CellPositionChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.CellPosition), ResourceType = typeof(BusinessObjects))]
        public int CellPositionId { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        public IEnumerable<CompartmentDetails> Compartments { get; set; }

        public int Id { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitLength), ResourceType = typeof(BusinessObjects))]
        public int Length
        {
            get => this.length;
            set => this.SetIfStrictlyPositive(ref this.length, value);
        }

        public IEnumerable<Enumeration<string>> LoadingUnitStatusChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitStatus), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitStatusId { get; set; }

        public IEnumerable<Enumeration<int>> LoadingUnitTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitType), ResourceType = typeof(BusinessObjects))]
        public int LoadingUnitTypeId { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWidth), ResourceType = typeof(BusinessObjects))]
        public int Width
        {
            get => this.width;
            set => this.SetIfStrictlyPositive(ref this.width, value);
        }

        #endregion Properties
    }
}
