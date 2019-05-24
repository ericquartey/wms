namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IOrderableCompartmentSet : IOrderableCompartment
    {
        #region Properties

        int Size { get; }

        #endregion
    }
}
