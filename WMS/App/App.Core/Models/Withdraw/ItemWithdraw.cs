using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class ItemWithdraw : BusinessObject
    {
        #region Fields

        private IEnumerable<Area> areaChoices;

        private int? areaId;

        private IEnumerable<Bay> bayChoices;

        private int? bayId;

        private ItemDetails itemDetails;

        private int quantity;

        #endregion

        #region Properties

        public IEnumerable<Area> AreaChoices
        {
            get => this.areaChoices;
            set => this.SetProperty(ref this.areaChoices, value);
        }

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

        [Display(Name = nameof(BusinessObjects.ItemWithdrawBay), ResourceType = typeof(BusinessObjects))]
        public int? BayId
        {
            get => this.bayId;
            set => this.SetProperty(ref this.bayId, value);
        }

        public override string Error => string.Join(Environment.NewLine, new[]
            {
                this[nameof(this.ItemDetails)],
                this[nameof(this.AreaId)],
                this[nameof(this.BayId)],
                this[nameof(this.Quantity)],
            }.Where(s => !string.IsNullOrEmpty(s)));

        [Display(Name = nameof(BusinessObjects.ItemWithdrawItem), ResourceType = typeof(BusinessObjects))]
        public ItemDetails ItemDetails
        {
            get => this.itemDetails;
            set => this.SetProperty(ref this.itemDetails, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawQuantity), ResourceType = typeof(BusinessObjects))]
        public int Quantity
        {
            get => this.quantity;
            set => this.SetIfPositive(ref this.quantity, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawRegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemWithdrawSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.AreaId):
                        if (this.areaId.HasValue == false ||
                            this.areaId.Value == 0)
                        {
                            return BusinessObjects.ItemWithdrawAreaInvalidError;
                        }

                        break;

                    case nameof(this.BayId):
                        if (this.bayId.HasValue == false ||
                            this.bayId.Value == 0)
                        {
                            return BusinessObjects.ItemWithdrawBayInvalidError;
                        }

                        break;

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

                return string.Empty;
            }
        }

        #endregion
    }
}
