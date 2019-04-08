using System;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class SchedulerRequest : Model
    {
        #region Fields

        public const int InstantRequestPriority = 1;

        private int requestedQuantity;

        private int reservedQuantity;

        #endregion

        #region Properties

        public int AreaId { get; set; }

        public int? BayId { get; set; }

        public DateTime? CreationDate { get; set; }

        public bool IsInstant { get; set; }

        public int ItemId { get; set; }

        public int? ListId { get; set; }

        public int? ListRowId { get; set; }

        public int? LoadingUnitId { get; set; }

        public int? LoadingUnitTypeId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public int? Priority { get; set; }

        public int QuantityLeftToReserve => this.requestedQuantity - this.reservedQuantity;

        public string RegistrationNumber { get; set; }

        public int RequestedQuantity
        {
            get => this.requestedQuantity;
            set
            {
                if (value < this.reservedQuantity)
                {
                    throw new ArgumentOutOfRangeException($"The requested quantity cannot be lower than the reserved quantity.");
                }

                SetIfPositive(ref this.requestedQuantity, value);
            }
        }

        public int ReservedQuantity
        {
            get => this.reservedQuantity;
            set
            {
                if (value > this.requestedQuantity)
                {
                    throw new ArgumentOutOfRangeException($"The reserved quantity cannot be greater than the requested quantity.");
                }

                SetIfPositive(ref this.reservedQuantity, value);
            }
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
