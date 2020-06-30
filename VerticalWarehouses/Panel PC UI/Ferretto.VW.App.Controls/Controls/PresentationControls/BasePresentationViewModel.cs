using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls
{
    public class BasePresentationViewModel : BindableBase, IPresentation
    {
        #region Fields

        private readonly IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

        private DelegateCommand executeCommand;

        private bool? isEnabled;

        private bool? isVisible;

        private PresentationStates state;

        private PresentationTypes type;

        #endregion

        #region Constructors

        protected BasePresentationViewModel(PresentationTypes type)
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

        public bool? IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

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

        public virtual Task OnLoadedAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual bool CanExecute()
        {
            return true;
        }

        protected virtual void RaiseCanExecuteChanged()
        {
            this.executeCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
