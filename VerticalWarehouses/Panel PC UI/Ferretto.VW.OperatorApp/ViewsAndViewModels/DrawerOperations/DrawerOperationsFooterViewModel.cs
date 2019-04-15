using System;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerOperationsFooterViewModel : BindableBase, IDrawerOperationsFooterViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand backToDrawerOperationsMainView;

        private IUnityContainer container;

        #endregion

        #region Constructors

        public DrawerOperationsFooterViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public CompositeCommand BackButtonCommand { get; set; }

        public ICommand BackToDrawerOperationsMainView => this.backToDrawerOperationsMainView ?? (this.backToDrawerOperationsMainView = new DelegateCommand(
            () =>
    {
        this.container.Resolve<IDrawerOperationsMainViewModel>().NavigateToView<DrawerActivityViewModel, IDrawerActivityViewModel>();
    }
    ));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeButtons()
        {
            this.BackButtonCommand = new CompositeCommand();
            this.BackButtonCommand.RegisterCommand(this.container.Resolve<IDrawerOperationsMainViewModel>().BackToMainWindowNavigationButtonsViewButtonCommand());
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
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
