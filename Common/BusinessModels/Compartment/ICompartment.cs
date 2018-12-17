using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.BusinessModels
{
    public interface ICompartment : IBusinessObject
    {
        #region Properties

        int? Height { get; set; }

        int LoadingUnitId { get; set; }

        int? Width { get; set; }

        int? XPosition { get; set; }

        int? YPosition { get; set; }

        #endregion Properties
    }
}
