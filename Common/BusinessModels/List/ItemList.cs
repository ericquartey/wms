using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class ItemList : BusinessObject
    {
        #region Fields

        private string code;
        private DateTime creationDate;
        private string description;
        private int itemListItemsCount;
        private int itemListRowsCount;
        private ItemListStatus itemListStatus;
        private ItemListType itemListType;
        private int priority;

        #endregion

        #region Properties

        [Display(Name = nameof(General.Code), ResourceType = typeof(General))]
        public string Code { get => this.code; set => this.SetProperty(ref this.code, value); }

        [Display(Name = nameof(General.CreationDate), ResourceType = typeof(General))]
        public DateTime CreationDate { get => this.creationDate; set => this.SetProperty(ref this.creationDate, value); }

        [Display(Name = nameof(General.Description), ResourceType = typeof(General))]
        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        [Display(Name = nameof(BusinessObjects.ItemListItemsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemListItemsCount
        {
            get => this.itemListItemsCount;
            set => this.SetProperty(ref this.itemListItemsCount, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemListRowsCount), ResourceType = typeof(BusinessObjects))]
        public int ItemListRowsCount { get => this.itemListRowsCount; set => this.SetProperty(ref this.itemListRowsCount, value); }

        [Display(Name = nameof(BusinessObjects.ItemListStatus), ResourceType = typeof(BusinessObjects))]
        public ItemListStatus ItemListStatus { get => this.itemListStatus; set => this.SetProperty(ref this.itemListStatus, value); }

        [Display(Name = nameof(General.Type), ResourceType = typeof(General))]
        public ItemListType ItemListType { get => this.itemListType; set => this.SetProperty(ref this.itemListType, value); }

        [Display(Name = nameof(BusinessObjects.ItemListPriority), ResourceType = typeof(BusinessObjects))]
        public int Priority { get => this.priority; set => this.SetProperty(ref this.priority, value); }

        #endregion
    }
}
