using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class ItemAreaInput : BusinessObject
    {
        #region Fields

        private int? areaId;

        #endregion

        #region Properties

        [Display(Name = nameof(BusinessObjects.Area), ResourceType = typeof(BusinessObjects))]
        public int? AreaId
        {
            get => this.areaId;
            set
            {
                this.SetProperty(ref this.areaId, value);
            }
        }

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
                    case nameof(this.AreaId):

                        return this.GetErrorMessageIfZeroOrNull(this.AreaId, columnName);
                }

                return null;
            }
        }

        #endregion
    }
}
