using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls
{
    public class BasePresentation : BindableBase, IPresentation
    {
        #region Fields

        private readonly IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

        private DelegateCommand executeCommand;

        private bool? isVisible;

        private PresentationStates state;

        private PresentationTypes type;

        #endregion

        #region Constructors

        protected BasePresentation(PresentationTypes type)
        {
            this.Type = type;
        }

        #endregion

        #region Properties

        public IEventAggregator EventAggregator => this.eventAggregator;

        public ICommand ExecuteCommand =>
            this.executeCommand
            ??
            (this.executeCommand = new DelegateCommand(async () => await this.ExecuteAsync(), this.CanExecute));

        public bool? IsVisible
        {
            get => this.isVisible;
            set => this.SetProperty(ref this.isVisible, value);
        }

        public PresentationStates State
        {
            get => this.state;
            set => this.SetProperty(ref this.state, value);
        }

        public PresentationTypes Type
        {
            get => this.type;
            set => this.SetProperty(ref this.type, value);
        }

        #endregion

        #region Methods

        public virtual Task ExecuteAsync()
        {
            // do nothing
            return Task.CompletedTask;
        }

        protected virtual bool CanExecute()
        {
            return true;
        }

        protected void RaiseCanExecuteChanged()
        {
            this.executeCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
