using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class CompartmentTypeInput : BusinessObject
    {
        #region Fields

        private int? itemId;

        private double? maxCapacity;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.Item), ResourceType = typeof(BusinessObjects))]
        public int? ItemId { get => this.itemId; set => this.SetProperty(ref this.itemId, value); }

        [Display(Name = nameof(BusinessObjects.ItemMaxCapacity), ResourceType = typeof(BusinessObjects))]
        public double? MaxCapacity { get => this.maxCapacity; set => this.SetProperty(ref this.maxCapacity, value); }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                if (!this.IsValidationEnabled)
                {
                    return null;
                }

                var baseError = base[columnName];
                if (!string.IsNullOrEmpty(baseError))
                {
                    return baseError;
                }

                switch (columnName)
                {
                    case nameof(this.ItemId):

                        return this.GetErrorMessageIfZeroOrNull(this.ItemId, columnName);

                    case nameof(this.MaxCapacity):

                        return this.GetErrorMessageIfZeroOrNull((int?)this.MaxCapacity, columnName);
                }

                return null;
            }
        }

        #endregion
    }
}
