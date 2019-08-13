using System.Windows.Input;

namespace Ferretto.VW.App.Services.Interfaces
{
    public interface IPresentation
    {
        #region Properties

        ICommand ExecuteCommand { get; }

        bool? IsVisible { get; set; }

        PresentationStates State { get; set; }

        PresentationTypes Type { get; }

        #endregion
    }
}
