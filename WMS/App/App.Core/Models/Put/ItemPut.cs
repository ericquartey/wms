using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class ItemPut : BusinessObject
    {
        #region Fields

        private IEnumerable<Area> areaChoices;

        private int? areaId;

        private IEnumerable<Bay> bayChoices;

        private int? bayId;

        private ItemDetails itemDetails;

        private string lot;

        private int? quantity;

        private string registrationNumber;

        private string sub1;

        private string sub2;

        private IEnumerable<Enumeration> materialStatusChoices;

        private int? materialStatusId;

        private IEnumerable<Enumeration> packageTypeChoices;

        private int? packageTypeId;
        #endregion

        #region Properties

        public IEnumerable<Area> AreaChoices
        {
            get => this.areaChoices;
            set => this.SetProperty(ref this.areaChoices, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemWithdrawArea), ResourceType = typeof(BusinessObjects))]
        public int? AreaId
        {
            get => this.areaId;
            set => this.SetProperty(ref this.areaId, value);
        }

        public IEnumerable<Bay> BayChoices
        {
            get => this.bayChoices;
            set => this.SetProperty(ref this.bayChoices, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemWithdrawBay), ResourceType = typeof(BusinessObjects))]
        public int? BayId
        {
            get => this.bayId;
            set => this.SetProperty(ref this.bayId, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawItem), ResourceType = typeof(BusinessObjects))]
        public ItemDetails ItemDetails
        {
            get => this.itemDetails;
            set => this.SetProperty(ref this.itemDetails, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get => this.lot; set => this.SetProperty(ref this.lot, value); }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemWithdrawQuantity), ResourceType = typeof(BusinessObjects))]
        public int? Quantity
        {
            get => this.quantity;
            set => this.SetProperty(ref this.quantity, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawRegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get => this.registrationNumber; set => this.SetProperty(ref this.registrationNumber, value); }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get => this.sub1; set => this.SetProperty(ref this.sub1, value); }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get => this.sub2; set => this.SetProperty(ref this.sub2, value); }

        public IEnumerable<Enumeration> MaterialStatusChoices
        {
            get => this.materialStatusChoices;
            set => this.SetProperty(ref this.materialStatusChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.MaterialStatus), ResourceType = typeof(BusinessObjects))]
        public int? MaterialStatusId { get => this.materialStatusId; set => this.SetProperty(ref this.materialStatusId, value); }

        public IEnumerable<Enumeration> PackageTypeChoices
        {
            get => this.packageTypeChoices;
            set => this.SetProperty(ref this.packageTypeChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.PackageType), ResourceType = typeof(BusinessObjects))]
        public int? PackageTypeId { get => this.packageTypeId; set => this.SetProperty(ref this.packageTypeId, value); }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                if (!this.IsValidationEnabled)
                {
                    return null;
                }

                var baseError = base[columnName];
                if (!string.IsNullOrEmpty(baseError))
                {
                    return baseError;
                }

                switch (columnName)
                {
                    case nameof(this.AreaId):
                        return GetErrorMessageIfZeroOrNull(this.AreaId, nameof(this.AreaId));

                    case nameof(this.BayId):
                        return GetErrorMessageIfZeroOrNull(this.BayId, nameof(this.BayId));

                    case nameof(this.Quantity):
                        if (this.Quantity <= 0 || this.Quantity > this.ItemDetails?.TotalAvailable)
                        {
                            return BusinessObjects.ItemWithdrawQuantityInvalidError;
                        }

                        break;

                    case nameof(this.ItemDetails):
                        if (this.ItemDetails == null)
                        {
                            return BusinessObjects.ItemWithdrawItemDetailsInvalidError;
                        }

                        break;
                }

                return null;
            }
        }

        #endregion
    }
}
