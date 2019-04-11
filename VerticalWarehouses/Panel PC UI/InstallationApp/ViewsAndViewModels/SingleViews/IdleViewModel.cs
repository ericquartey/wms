using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.Utils.Interfaces;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp
{
    public class IdleViewModel : BindableBase, IViewModel, IIdleViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public IdleViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
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
