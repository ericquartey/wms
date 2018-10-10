using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Modules.BLL.Models
{
    public sealed class Compartment : BusinessObject<int>
    {
        #region Fields

        private int stock;

        #endregion Fields

        #region Constructors

        public Compartment(int id) : base(id)
        { }

        #endregion Constructors

        #region Properties

        [Display(Name = nameof(BusinessObjects.CompartmentCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentStatus), ResourceType = typeof(BusinessObjects))]
        public string CompartmentStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentType), ResourceType = typeof(BusinessObjects))]
        public string CompartmentTypeDescription { get; set; }

        public int Id { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemDescription_extended), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentPairing), ResourceType = typeof(BusinessObjects))]
        public string ItemPairing { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode_extended), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitCode { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public string MaterialStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentStock), ResourceType = typeof(BusinessObjects))]
        public int Stock
        {
            get => this.stock;
            set => this.SetIfPositive(ref this.stock, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.CompartmentSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

        #endregion Properties
    }
}
