using System.Threading.Tasks;

namespace Ferretto.WMS.App.Controls.Interfaces
{
    public interface IExtensionDataEntityViewModel
    {
        #region Properties

        ColorRequired ColorRequired { get; }

        #endregion

        #region Methods

        Task LoadRelatedDataAsync();

        #endregion
    }
}
