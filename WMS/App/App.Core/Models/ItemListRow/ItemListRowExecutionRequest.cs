using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class ItemListRowExecutionRequest : BusinessObject
    {
        #region Fields

        private IEnumerable<Area> areaChoices;

        private int? areaId;

        private bool areaIdHasValue;

        private IEnumerable<Bay> bayChoices;

        private int? bayId;

        private ItemListRowDetails itemListRowDetails;

        private bool schedule;

        #endregion

        #region Properties

        public IEnumerable<Area> AreaChoices
        {
            get => this.areaChoices;
            set => this.SetProperty(ref this.areaChoices, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.ItemListExecutionRequestArea), ResourceType = typeof(BusinessObjects))]
        public int? AreaId
        {
            get => this.areaId;
            set
            {
                if (this.SetProperty(ref this.areaId, value))
                {
                    this.AreaIdHasValue = this.areaId.HasValue;
                }
            }
        }

        public bool AreaIdHasValue
        {
            get => this.areaIdHasValue;
            set => this.SetProperty(ref this.areaIdHasValue, value);
        }

        public IEnumerable<Bay> BayChoices
        {
            get => this.bayChoices;
            set => this.SetProperty(ref this.bayChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListExecutionRequestBay), ResourceType = typeof(BusinessObjects))]
        public int? BayId
        {
            get => this.bayId;
            set => this.SetProperty(ref this.bayId, value);
        }

        public ItemListRowDetails ItemListRowDetails
        {
            get => this.itemListRowDetails;
            set => this.SetProperty(ref this.itemListRowDetails, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListExecutionRequestSchedule), ResourceType = typeof(BusinessObjects))]
        public bool Schedule
        {
            get => this.schedule;
            set
            {
                this.SetProperty(ref this.schedule, value);
                if (value)
                {
                    this.BayId = null;
                }

                this.RaisePropertyChanged(nameof(this.BayId));
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
                        if (!this.areaId.HasValue ||
                            this.areaId.Value == 0)
                        {
                            return this.GetErrorMessageForInvalid(columnName);
                        }

                        break;

                    case nameof(this.BayId):
                        if ((!this.bayId.HasValue ||
                            this.bayId.Value == 0) && !this.schedule)
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
