using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsGeneralDataViewModel : BindableBase, IStatisticsGeneralDataViewModel
    {
        #region Fields

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public StatisticsGeneralDataViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
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

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.NavigationViewModel = this.container.Resolve<IStatisticsNavigationViewModel>() as StatisticsNavigationViewModel;
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
