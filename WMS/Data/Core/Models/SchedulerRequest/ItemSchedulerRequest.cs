using System;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(SchedulerRequest))]
    public class ItemSchedulerRequest : BaseModel<int>, ISchedulerRequest
    {
        #region Fields

        private double requestedQuantity;

        private double reservedQuantity;

        #endregion

        #region Properties

        public int AreaId { get; set; }

        public int? BayId { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsInstant { get; set; }

        public int ItemId { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public OperationType OperationType { get; set; }

        public int? PackageTypeId { get; set; }

        [Positive]
        public int? Priority { get; set; }

        public double QuantityLeftToReserve => this.requestedQuantity - this.reservedQuantity;

        public string RegistrationNumber { get; set; }

        [PositiveOrZero]
        public double RequestedQuantity
        {
            get => this.requestedQuantity;
            set
            {
                if (value < this.reservedQuantity)
                {
                    throw new ArgumentOutOfRangeException(WMS.Data.Resources.SchedulerRequest.ItemSchedulerRequestArgumentExceptionLower);
                }

                this.requestedQuantity = value;
            }
        }

        [PositiveOrZero]
        public double ReservedQuantity
        {
            get => this.reservedQuantity;
            set
            {
                if (value > this.requestedQuantity)
                {
                    throw new ArgumentOutOfRangeException(WMS.Data.Resources.SchedulerRequest.ItemSchedulerRequestArgumentExceptionGreater);
                }

                this.reservedQuantity = value;
            }
        }

        public SchedulerRequestStatus Status { get; set; } = SchedulerRequestStatus.New;

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public virtual SchedulerRequestType Type => SchedulerRequestType.Item;

        #endregion

        #region Methods

        public static ItemSchedulerRequest FromPickOptions(int itemId, ItemOptions options, ItemListRowOperation row)
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
            request.OperationType = OperationType.Pick;

            return request;
        }

        public static ItemSchedulerRequest FromPutOptions(int itemId, ItemOptions options, ItemListRowOperation row)
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
            request.OperationType = OperationType.Put;

            return request;
        }

        #endregion
    }
}
