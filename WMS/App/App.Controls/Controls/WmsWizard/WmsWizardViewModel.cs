using System.Windows.Input;
using Ferretto.WMS.App.Controls.Services;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public class WmsWizardViewModel : BaseServiceNavigationViewModel, IWmsWizardViewModel
    {
        #region Fields

        private ICommand cancelCommand;

        private string error;

        private ICommand goToNextCommand;

        private ICommand goToPreviousCommand;

        private bool isSaveVisible;

        private ICommand saveCommand;

        #endregion

        #region Properties

        public ICommand CancelCommand => this.cancelCommand ??
                                         (this.cancelCommand = new DelegateCommand(this.Cancel));

        public string Error
        {
            get => this.error;
            set => this.SetProperty(ref this.error, value);
        }

        public ICommand GoToNextCommand => this.goToNextCommand ??
                                (this.goToNextCommand = new DelegateCommand(this.GoToNext, this.CanGoToNext));

        public ICommand GoToPreviousCommand => this.goToPreviousCommand ??
                        (this.goToPreviousCommand = new DelegateCommand(this.GoToPrevious, this.CanGoToPrevious));

        public bool IsSaveVisible
        {
            get => this.isSaveVisible;
            set => this.SetProperty(ref this.isSaveVisible, value);
        }

        public ICommand SaveCommand => this.saveCommand ??
                        (this.saveCommand = new DelegateCommand(this.Save, this.CanSave));

        #endregion

        #region Methods

        public void Refresh()
        {
            ((DelegateCommand)this.GoToNextCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.GoToPreviousCommand).RaiseCanExecuteChanged();
        }

        public void SetIsSaveVisible(bool isSaveVisible)
        {
            this.IsSaveVisible = isSaveVisible;
        }

        public void UpdateCanSave()
        {
            ((DelegateCommand)this.SaveCommand).RaiseCanExecuteChanged();
        }

        public void UpdateError(string errorInfo)
        {
            this.Error = errorInfo;
        }

        internal virtual void Cancel()
        {
            this.OnCommandExecute(CommandExecuteType.Cancel);
        }

        internal virtual void GoToNext()
        {
            this.OnCommandExecute(CommandExecuteType.Next);
        }

        internal virtual void GoToPrevious()
        {
            this.OnCommandExecute(CommandExecuteType.Previous);
        }

        internal virtual void Save()
        {
            this.OnCommandExecute(CommandExecuteType.Save);
        }

        protected virtual bool OnCommandExecute(CommandExecuteType command)
        {
            var pubEvent = new StepsPubSubEvent(command);
            this.EventService.Invoke(pubEvent);
            return pubEvent.CanExecute;
        }

        private bool CanGoToNext()
        {
            var canGoToNext = this.OnCommandExecute(CommandExecuteType.CanNext);
            return canGoToNext;
        }

        private bool CanGoToPrevious()
        {
            return this.OnCommandExecute(CommandExecuteType.CanPrevious);
        }

        private bool CanSave()
        {
            return this.OnCommandExecute(CommandExecuteType.CanSave);
        }

        #endregion
    }
}
