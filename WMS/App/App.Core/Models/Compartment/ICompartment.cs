using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Core.Models
{
    public interface ICompartment : IModel<int>
    {
        #region Properties

        int? Height { get; set; }

        int LoadingUnitId { get; set; }

        int? Width { get; set; }

        int? XPosition { get; set; }

        int? YPosition { get; set; }

        #endregion
    }
}
