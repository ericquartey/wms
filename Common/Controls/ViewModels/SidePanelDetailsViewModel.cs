using System;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public abstract class SidePanelDetailsViewModel<T> : DetailsViewModel<T>
        where T : BusinessObject
    {
        #region Fields

        private ICommand cancelCommand;

        private bool isOpen;
        private string title;

        #endregion Fields

        #region Events

        public event EventHandler<OperationEventArgs<T>> OperationComplete;

        #endregion Events

        #region Properties

        public ICommand CancelCommand => this.cancelCommand ??
                                  (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand));

        public bool IsOpen
        {
            get => this.isOpen;
            set => this.SetProperty(ref this.isOpen, value);
        }

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        #endregion Properties

        #region Methods

        public void Hide()
        {
            this.IsOpen = false;
        }

        public void Show()
        {
            this.IsOpen = true;
        }

        protected virtual void CompleteOperation()
        {
            this.OperationComplete?.Invoke(this, new OperationEventArgs<T>(this.Model));
        }

        private void ExecuteCancelCommand()
        {
            this.CompleteOperation();
        }

        #endregion Methods
    }
}
