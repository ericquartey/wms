using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.Resources;
using Ferretto.VW.OperatorApp.Resources.Enumerations;
using Ferretto.VW.Utils.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsMainViewModel : BindableBase, IStatisticsMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private BindableBase statisticsContentRegion;

        private BindableBase statisticsNavigationRegion;

        #endregion

        #region Constructors

        public StatisticsMainViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public BindableBase StatisticsContentRegion { get => this.statisticsContentRegion; set => this.SetProperty(ref this.statisticsContentRegion, value); }

        public BindableBase StatisticsNavigationRegion { get => this.statisticsNavigationRegion; set => this.SetProperty(ref this.statisticsNavigationRegion, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public void NavigateToView<T, I>()
            where T : BindableBase, I
            where I : IViewModel
        {
            this.eventAggregator.GetEvent<OperatorApp_Event>().Publish(new OperatorApp_EventMessage(OperatorApp_EventMessageType.EnterView));
            var desiredViewModel = this.container.Resolve<I>() as T;
            desiredViewModel.SubscribeMethodToEvent();
            this.container.Resolve<IMainWindowBackToOAPPButtonViewModel>().BackButtonCommand.RegisterCommand(new DelegateCommand(desiredViewModel.ExitFromViewMethod));
            this.StatisticsContentRegion = desiredViewModel;
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
