using System.Threading.Tasks;
using Ferretto.VW.App.Operator.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.Other
{
    public class DrawerCompactingDetailViewModel : BindableBase, IDrawerCompactingDetailViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public DrawerCompactingDetailViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

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
