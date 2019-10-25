using System.Windows.Input;

namespace Ferretto.VW.App.Services
{
    public interface IPresentation
    {
        #region Properties

        ICommand ExecuteCommand { get; }

        bool? IsEnabled { get; set; }

        bool? IsVisible { get; set; }

        PresentationStates State { get; set; }

        PresentationTypes Type { get; set; }

        #endregion
    }
}
