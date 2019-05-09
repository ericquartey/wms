namespace Ferretto.WMS.App.Core.Models
{
    public interface ICapacityCompartment
    {
        #region Properties

        double? MaxCapacity { get; }

        double Stock { get; }

        #endregion
    }
}
