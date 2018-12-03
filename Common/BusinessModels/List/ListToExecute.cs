using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class ListToExecute : BusinessObject
    {
        #region Fields

        private IEnumerable<Area> areaChoices;
        private int? areaId;
        private IEnumerable<Bay> bayChoices;
        private int? bayId;
        private ItemListDetails itemListDetails;

        #endregion Fields

        #region Properties

        public IEnumerable<Area> AreaChoices
        {
            get => this.areaChoices;
            set => this.SetProperty(ref this.areaChoices, value);
        }

        [Display(Name = nameof(BusinessObjects.ListToExecuteArea), ResourceType = typeof(BusinessObjects))]
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

        [Display(Name = nameof(BusinessObjects.ListToExecuteBayOptional), ResourceType = typeof(BusinessObjects))]
        public int? BayId
        {
            get => this.bayId;
            set => this.SetProperty(ref this.bayId, value);
        }

        public override string Error => String.Join(Environment.NewLine, new[]
            {
                this[nameof(this.ItemListDetails)],
                this[nameof(this.AreaId)],
                this[nameof(this.BayId)],
            }.Where(s => !String.IsNullOrEmpty(s))
        );

        [Display(Name = nameof(BusinessObjects.ItemCode), ResourceType = typeof(BusinessObjects))]
        public string ItemCode => this.ItemListDetails?.Code;

        [Display(Name = nameof(BusinessObjects.ItemDescription), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription => this.ItemListDetails?.Description;

        public ItemListDetails ItemListDetails
        {
            get => this.itemListDetails;
            set => this.SetProperty(ref this.itemListDetails, value);
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
                            return Resources.BusinessObjects.ListToExecuteAreaInvalidError;
                        }
                        break;

                    case nameof(this.BayId):
                        if (this.bayId.HasValue == false ||
                            this.bayId.Value == 0)
                        {
                            return Resources.BusinessObjects.ListToExecuteBayInvalidError;
                        }
                        break;
                }

                return String.Empty;
            }
        }

        #endregion Indexers
    }
}
