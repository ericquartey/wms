using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.Interfaces
{
    public interface ILSMTMainViewModel : IViewModel
    {
        #region Properties

        BindableBase LSMTContentRegionCurrentViewModel { get; set; }

        #endregion
    }
}
