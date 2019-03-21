using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BusinessModels
{
    public interface ICompartment : IModel<int>
    {
        #region Properties

        double? Height { get; set; }

        int LoadingUnitId { get; set; }

        double? Width { get; set; }

        double? XPosition { get; set; }

        double? YPosition { get; set; }

        #endregion
    }
}
