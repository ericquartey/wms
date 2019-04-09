using System;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class ItemSchedulerRequest : Model, ISchedulerRequest
    {
        #region Fields

        private int requestedQuantity;

        private int reservedQuantity;

        #endregion

        #region Properties

        public int AreaId { get; set; }

        public int? BayId { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsInstant { get; set; }

        public int ItemId { get; set; }

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

        public virtual SchedulerType SchedulerType { get => SchedulerType.Item; }

        public SchedulerRequestStatus Status { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public OperationType Type { get; set; }

        #endregion

        #region Methods

        public static LoadingUnitSchedulerRequest FromLoadingUnitWithdrawalOptions(int loadingUnitId)
        {
            return new LoadingUnitSchedulerRequest
            {
                IsInstant = true,
                LoadingUnitId = loadingUnitId
            };
        }

        public static ItemSchedulerRequest FromWithdrawalOptions(int itemId, ItemWithdrawOptions options, ItemListRow row)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            ItemSchedulerRequest request = null;

            if (row == null)
            {
                request = new ItemSchedulerRequest();
            }
            else
            {
                request = new ItemListRowSchedulerRequest
                {
                    ListId = row.ListId,
                    ListRowId = row.Id
                };
            }

            request.AreaId = options.AreaId;
            request.BayId = options.BayId;
            request.IsInstant = options.RunImmediately;
            request.ItemId = itemId;
            request.Lot = options.Lot;
            request.MaterialStatusId = options.MaterialStatusId;
            request.PackageTypeId = options.PackageTypeId;
            request.RegistrationNumber = options.RegistrationNumber;
            request.RequestedQuantity = options.RequestedQuantity;
            request.Sub1 = options.Sub1;
            request.Sub2 = options.Sub2;
            request.Type = OperationType.Withdrawal;

            return request;
        }

        #endregion
    }
}
