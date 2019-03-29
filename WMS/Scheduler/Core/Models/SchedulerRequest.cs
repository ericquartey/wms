using System;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class SchedulerRequest : Model
    {
        #region Fields

        public const int InstantRequestPriority = 1;

        private int dispatchedQuantity;

        private int requestedQuantity;

        #endregion

        #region Properties

        public int AreaId { get; set; }

        public int? BayId { get; set; }

        public System.DateTime? CreationDate { get; set; }

        public int DispatchedQuantity
        {
            get => this.dispatchedQuantity;
            set
            {
                if (value > this.requestedQuantity)
                {
                    throw new System.ArgumentOutOfRangeException($"The {nameof(this.DispatchedQuantity)} cannot be greater than {nameof(this.RequestedQuantity)}");
                }

                this.dispatchedQuantity = value;
            }
        }

        public bool IsInstant { get; set; }

        public int ItemId { get; set; }

        public int? ListId { get; set; }

        public int? ListRowId { get; set; }

        public ItemListRowStatus ListRowStatus { get; set; }

        public int? LoadingUnitId { get; set; }

        public int? LoadingUnitTypeId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public int? Priority { get; set; }

        public int QuantityLeftToDispatch => this.requestedQuantity - this.dispatchedQuantity;

        public string RegistrationNumber { get; set; }

        public int RequestedQuantity
        {
            get => this.requestedQuantity;
            set => SetIfStrictlyPositive(ref this.requestedQuantity, value);
        }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public OperationType Type { get; set; }

        #endregion

        #region Methods

        public static SchedulerRequest FromWithdrawalOptions(int itemId, ItemWithdrawOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new SchedulerRequest
            {
                AreaId = options.AreaId,
                BayId = options.BayId,
                IsInstant = options.RunImmediately,
                ItemId = itemId,
                Lot = options.Lot,
                MaterialStatusId = options.MaterialStatusId,
                PackageTypeId = options.PackageTypeId,
                RegistrationNumber = options.RegistrationNumber,
                RequestedQuantity = options.RequestedQuantity,
                Sub1 = options.Sub1,
                Sub2 = options.Sub2,
                Type = OperationType.Withdrawal
            };
        }

        #endregion
    }
}
