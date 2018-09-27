namespace Ferretto.Common.Modules.BLL.Models
{
    public class Compartment : BusinessObject
    {
        #region Fields

        private int stock;

        #endregion Fields

        #region Properties

        public string Code { get; set; }
        public string CompartmentStatusDescription { get; set; }
        public string CompartmentTypeDescription { get; set; }

        public int Id { get; set; }

        public string ItemDescription { get; set; }

        public string LoadingUnitCode { get; set; }
        public string Lot { get; set; }
        public string MaterialStatusDescription { get; set; }

        public int Stock
        {
            get => this.stock;
            set => SetIfPositive(ref this.stock, value);
        }

        public string Sub1 { get; set; }
        public string Sub2 { get; set; }

        #endregion Properties
    }
}
