using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.Resources;
using Ferretto.VW.OperatorApp.Resources.Enumerations;
using Ferretto.VW.Utils.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerOperationsMainViewModel : BindableBase, IDrawerOperationsMainViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand backToMainWindowNavigationButtonsViewButtonCommand;

        private IUnityContainer container;

        private BindableBase drawerOperationsContentRegion;

        #endregion

        #region Constructors

        public DrawerOperationsMainViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public BindableBase DrawerOperationsContentRegion { get => this.drawerOperationsContentRegion; set => this.SetProperty(ref this.drawerOperationsContentRegion, value); }

        #endregion

        #region Methods

        public ICommand BackToMainWindowNavigationButtonsViewButtonCommand() => this.backToMainWindowNavigationButtonsViewButtonCommand ??
            new DelegateCommand(() => { this.NavigateToView<DrawerActivityViewModel, IDrawerActivityViewModel>(); });

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
            this.DrawerOperationsContentRegion = desiredViewModel;
        }

        public void SubscribeMethodToEvent()
        {
            this.DrawerOperationsContentRegion = this.container.Resolve<IDrawerActivityViewModel>() as DrawerActivityViewModel;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
