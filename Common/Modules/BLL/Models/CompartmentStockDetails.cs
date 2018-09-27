namespace Ferretto.Common.Modules.BLL.Models
{
    public class CompartmentStockDetails : BusinessObject
    {
        #region Fields

        private int reservedForPick;
        private int reservedToStore;
        private int stock;

        #endregion Fields

        #region Properties

        public int ReservedForPick
        {
            get => this.reservedForPick;
            set => SetIfPositive(ref this.reservedForPick, value);
        }

        public int ReservedToStore
        {
            get => this.reservedToStore;
            set => SetIfPositive(ref this.reservedToStore, value);
        }

        public int Stock
        {
            get => this.stock;
            set => SetIfPositive(ref this.stock, value);
        }

        #endregion Properties
    }
}
