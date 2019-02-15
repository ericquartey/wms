using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class Mission : BaseModel<int>
    {
        #region Fields

        private int quantity;

        #endregion

        #region Properties

        public string BayDescription { get; set; }

        public int? BayId { get; set; }

        public string CellAisleName { get; set; }

        public int? CellId { get; set; }

        public int? CompartmentId { get; set; }

        public int? CompartmentTypeHeight { get; set; }

        public int? CompartmentTypeWidth { get; set; }

        public DateTime CreationDate { get; set; }

        public string ItemDescription { get; set; }

        public int? ItemId { get; set; }

        public string ItemListDescription { get; set; }

        public int? ItemListId { get; set; }

        public string ItemListRowCode { get; set; }

        public int? ItemListRowId { get; set; }

        public string ItemMeasureUnitDescription { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public string LoadingUnitCode { get; set; }

        public int? LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public string MaterialStatusDescription { get; set; }

        public int? MaterialStatusId { get; set; }

        public string PackageTypeDescription { get; set; }

        public int? PackageTypeId { get; set; }

        public int? Priority { get; set; }

        public int Quantity
        {
            get => this.quantity;
            set => this.quantity = CheckIfStrictlyPositive(value);
        }

        public string RegistrationNumber { get; set; }

        public int RequiredQuantity { get; set; }

        public MissionStatus Status { get; set; } = MissionStatus.New;

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public MissionType Type { get; set; }

        #endregion
    }
}
