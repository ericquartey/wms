using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
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
        private bool runImmediately;

        #endregion Fields

        #region Properties

        public IEnumerable<Area> AreaChoices
        {
            get => this.areaChoices;
            set => this.SetProperty(ref this.areaChoices, value);
        }

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

        public override string Error => String.Join(Environment.NewLine, new[]
            {
                this[nameof(this.ItemListRowDetails)],
                this[nameof(this.AreaId)],
                this[nameof(this.BayId)],
            }.Where(s => !String.IsNullOrEmpty(s))
        );

        public ItemListRowDetails ItemListRowDetails
        {
            get => this.itemListRowDetails;
            set => this.SetProperty(ref this.itemListRowDetails, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListExecutionRequestRunImmediately), ResourceType = typeof(BusinessObjects))]
        public bool RunImmediately
        {
            get => this.runImmediately;
            set
            {
                if (this.SetProperty(ref this.runImmediately, value))
                {
                    this.BayId = null;
                }
            }
        }

        #endregion Properties

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
                            return Resources.BusinessObjects.ItemListExecutionAreaInvalidError;
                        }
                        break;

                    case nameof(this.BayId):
                        if ((this.bayId.HasValue == false ||
                            this.bayId.Value == 0) && this.runImmediately)
                        {
                            return Resources.BusinessObjects.ItemListExecutionBayInvalidError;
                        }
                        break;
                }

                return string.Empty;
            }
        }

        #endregion Indexers
    }
}
