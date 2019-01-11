using System;

namespace Ferretto.WMS.Data.WebAPI.Models
{
    public class ItemList : Model<int>
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
            set => this.itemListItemsCount = this.CheckIfPositive(value);
        }

        public int ItemListRowsCount
        {
            get => this.itemListRowsCount;
            set => this.itemListRowsCount = this.CheckIfPositive(value);
        }

        public ItemListStatus ItemListStatus { get; set; }

        public ItemListType ItemListType { get; set; }

        public int Priority
        {
            get => this.priority;
            set => this.priority = this.CheckIfPositive(value);
        }

        #endregion Properties
    }
}
