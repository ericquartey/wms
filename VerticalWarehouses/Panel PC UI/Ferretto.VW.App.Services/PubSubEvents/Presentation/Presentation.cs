using System.Windows.Input;
using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Services
{
    public class Presentation : IPresentation
    {
        #region Properties

        public ICommand ExecuteCommand { get; }

        public bool? IsEnabled { get; set; }

        public bool? IsVisible { get; set; }

        public PresentationStates State { get; set; }

        public PresentationTypes Type { get; set; }

        #endregion
    }
}
