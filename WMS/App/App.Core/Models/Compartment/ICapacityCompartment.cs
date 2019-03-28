namespace Ferretto.WMS.App.Core.Models
{
    public interface ICapacityCompartment
    {
        #region Properties

        int? MaxCapacity { get; }

        int Stock { get; }

        #endregion
    }
}
