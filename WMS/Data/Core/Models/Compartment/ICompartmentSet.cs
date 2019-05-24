using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface ICompartmentSet
    {
        #region Properties

        string Lot { get; set; }

        int? MaterialStatusId { get; set; }

        int? PackageTypeId { get; set; }

        string RegistrationNumber { get; set; }

        string Sub1 { get; set; }

        string Sub2 { get; set; }

        #endregion
    }
}
