using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemList : BaseModel<int>
    {
        #region Fields

        private int itemListItemsCount;

        private int itemListRowsCount;

        private int priority;

        #endregion Fields

        #region Properties

        public string Code { get; set; }

        public DateTime CreationDate { get; set; }

        public string Description { get; set; }

        public int ItemListItemsCount
        {
            get => this.itemListItemsCount;
            set => this.itemListItemsCount = CheckIfPositive(value);
        }

        public int ItemListRowsCount
        {
            get => this.itemListRowsCount;
            set => this.itemListRowsCount = CheckIfPositive(value);
        }

        public ItemListStatus ItemListStatus { get; set; }

        public ItemListType ItemListType { get; set; }

        public int Priority
        {
            get => this.priority;
            set => this.priority = CheckIfPositive(value);
        }

        #endregion Properties
    }
}
