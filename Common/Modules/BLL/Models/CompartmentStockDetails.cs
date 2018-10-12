using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.Modules.BLL.Models
{
    public sealed class CompartmentStockDetails : BusinessObject<int>
    {
        #region Fields

        private int reservedForPick;
        private int reservedToStore;
        private int stock;

        #endregion Fields

        #region Constructors

        public CompartmentStockDetails(int id) : base(id)
        { }

        #endregion Constructors

        #region Properties

        [Display(Name = nameof(BusinessObjects.CompartmentReservedForPick), ResourceType = typeof(BusinessObjects))]
        public int ReservedForPick
        {
            get => this.reservedForPick;
            set => this.SetIfPositive(ref this.reservedForPick, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentReservedToStore), ResourceType = typeof(BusinessObjects))]
        public int ReservedToStore
        {
            get => this.reservedToStore;
            set => this.SetIfPositive(ref this.reservedToStore, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentStock), ResourceType = typeof(BusinessObjects))]
        public int Stock
        {
            get => this.stock;
            set => this.SetIfPositive(ref this.stock, value);
        }

        #endregion Properties
    }
}
