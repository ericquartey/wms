namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICompartmentItemDetails
    {
        #region Properties

        string Lot { get; set; }

        int? MaterialStatusId { get; set; }

        int? PackageTypeId { get; set; }

        string RegistrationNumber { get; set; }

        double Stock { get; set; }

        string Sub1 { get; set; }

        string Sub2 { get; set; }

        #endregion
    }
}
