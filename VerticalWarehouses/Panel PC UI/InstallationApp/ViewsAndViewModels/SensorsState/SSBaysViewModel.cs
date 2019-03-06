using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSBaysViewModel : BindableBase, IViewModel, ISSBaysViewModel
    {
        #region Fields

        private IEventAggregator eventAggregator;

        private bool luPresentInBay;

        #endregion

        #region Constructors

        public SSBaysViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public bool LuPresentInBay { get => this.luPresentInBay; set => this.SetProperty(ref this.luPresentInBay, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
