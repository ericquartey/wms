using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityViewModel : BindableBase, IDrawerActivityViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private ICommand drawerDetailsButtonCommand;

        #endregion

        #region Constructors

        public DrawerActivityViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public ICommand DrawerDetailsButtonCommand => this.drawerDetailsButtonCommand ?? (this.drawerDetailsButtonCommand = new DelegateCommand(
            () =>
            {
                this.container.Resolve<IDrawerOperationsMainViewModel>().NavigateToView<DrawerWaitViewModel, IDrawerWaitViewModel>();
                this.container.Resolve<IMainWindowViewModel>().ChangeFooter<DrawerOperationsFooterViewModel, IDrawerOperationsFooterViewModel>();
            }));

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
