namespace Ferretto.WMS.Data.Core.Interfaces
{
    internal interface IOrderableCompartmentSet : IOrderableCompartment
    {
        #region Properties

        int Size { get; }

        #endregion
    }
}
