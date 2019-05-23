using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class LoadingUnitWithdraw : BusinessObject
    {
        #region Fields

        private int? areaId;

        private IEnumerable<Bay> bayChoices;

        private int? bayId;

        #endregion

        #region Properties

        public int? AreaId
        {
            get => this.areaId;
            set => this.SetProperty(ref this.areaId, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWithdrawArea), ResourceType = typeof(BusinessObjects))]
        public string AreaName { get; set; }

        public IEnumerable<Bay> BayChoices
        {
            get => this.bayChoices;
            set => this.SetProperty(ref this.bayChoices, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitWithdrawBay), ResourceType = typeof(BusinessObjects))]
        public int? BayId
        {
            get => this.bayId;
            set => this.SetProperty(ref this.bayId, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitStatus), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitType), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitTypeDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWithdrawLot), ResourceType = typeof(BusinessObjects))]
        public string Lot { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWithdrawRegistrationNumber), ResourceType = typeof(BusinessObjects))]
        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWithdrawSub1), ResourceType = typeof(BusinessObjects))]
        public string Sub1 { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWithdrawSub2), ResourceType = typeof(BusinessObjects))]
        public string Sub2 { get; set; }

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
                }

                return null;
            }
        }

        #endregion
    }
}
