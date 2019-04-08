using System;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class ItemSchedulerRequest : Model, ISchedulerRequest
    {
        #region Fields

        public const int InstantRequestPriority = 1;

        private int dispatchedQuantity;

        private int requestedQuantity;

        #endregion

        #region Properties

        public int AreaId { get; set; }

        public int? BayId { get; set; }

        public System.DateTime CreationDate { get; set; }

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

        public virtual SchedulerType SchedulerType { get => SchedulerType.Item; }

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
