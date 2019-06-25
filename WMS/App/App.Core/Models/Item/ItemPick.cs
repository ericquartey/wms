using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class ItemPick : BusinessObject
    {
        #region Fields

        private IEnumerable<Area> areaChoices;

        private int? areaId;

        private IEnumerable<Bay> bayChoices;

        private int? bayId;

        private bool isAreaIdSpecified;

        private ItemDetails itemDetails;

        private string lot;

        private IEnumerable<Enumeration> materialStatusChoices;

        private int? materialStatusId;

        private IEnumerable<Enumeration> packageTypeChoices;

        private int? packageTypeId;

        private double? quantity;

        private string registrationNumber;

        private string sub1;

        private string sub2;

        private double? totalAvailable;

        #endregion

        #region Properties

        public IEnumerable<Area> AreaChoices
        {
            get => this.areaChoices;
            set => this.SetProperty(ref this.areaChoices, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public int? AreaId
        {
            get => this.areaId;
            set
            {
                if (this.SetProperty(ref this.areaId, value))
                {
                    this.IsAreaIdSpecified = this.areaId.HasValue;
                }
            }
        }

        public IEnumerable<Bay> BayChoices
        {
            get => this.bayChoices;
            set => this.SetProperty(ref this.bayChoices, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.Bay), ResourceType = typeof(BusinessObjects))]
        public int? BayId
        {
            get => this.bayId;
            set => this.SetProperty(ref this.bayId, value);
        }

        public bool IsAreaIdSpecified
        {
            get => this.isAreaIdSpecified;
            set => this.SetProperty(ref this.isAreaIdSpecified, value);
        }

        [Display(Name = nameof(BusinessObjects.SelectedItem), ResourceType = typeof(BusinessObjects))]
        public ItemDetails ItemDetails
        {
            get => this.itemDetails;
            set => this.SetProperty(ref this.itemDetails, value);
        }

        [Display(Name = nameof(BusinessObjects.Lot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get => this.lot; set => this.SetProperty(ref this.lot, value); }

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

        [Required]
        [Display(Name = nameof(BusinessObjects.Quantity), ResourceType = typeof(BusinessObjects))]
        public double? Quantity
        {
            get => this.quantity;
            set
            {
                if (this.SetProperty(ref this.quantity, value))
                {
                    this.RaisePropertyChanged(nameof(this.RegistrationNumber));
                }
            }
        }

        [Display(Name = nameof(BusinessObjects.RegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber
        {
            get => this.registrationNumber;
            set
            {
                if (this.SetProperty(ref this.registrationNumber, value))
                {
                    this.RaisePropertyChanged(nameof(this.Quantity));
                }
            }
        }

        [Display(Name = nameof(BusinessObjects.Sub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get => this.sub1; set => this.SetProperty(ref this.sub1, value); }

        [Display(Name = nameof(BusinessObjects.Sub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get => this.sub2; set => this.SetProperty(ref this.sub2, value); }

        [Display(Name = nameof(BusinessObjects.ItemAvailable), ResourceType = typeof(BusinessObjects))]
        public double? TotalAvailable
        {
            get => this.totalAvailable;
            set
            {
                if (this.SetProperty(ref this.totalAvailable, value))
                {
                    this.RaisePropertyChanged(nameof(this.Quantity));
                }
            }
        }

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
                        return this.GetErrorMessageIfZeroOrNull(this.AreaId, columnName);

                    case nameof(this.BayId):
                        return this.GetErrorMessageIfZeroOrNull(this.BayId, columnName);

                    case nameof(this.Quantity):
                        if (this.Quantity <= 0 || this.Quantity > this.TotalAvailable)
                        {
                            return this.GetErrorMessageForInvalid(columnName);
                        }

                        if (!string.IsNullOrEmpty(this.RegistrationNumber) && this.Quantity > 1)
                        {
                            return Errors.QuantityMustBeOneIfRegistrationNumber;
                        }

                        break;

                    case nameof(this.TotalAvailable):
                        if (this.Quantity > this.TotalAvailable)
                        {
                            return this.GetErrorMessageForInvalid(columnName);
                        }

                        break;

                    case nameof(this.ItemDetails):
                        if (this.ItemDetails == null)
                        {
                            return this.GetErrorMessageForInvalid(columnName);
                        }

                        break;
                }

                return null;
            }
        }

        #endregion
    }
}
