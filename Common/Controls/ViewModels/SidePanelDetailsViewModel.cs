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

        private string title;

        #endregion Fields

        #region Events

        public event EventHandler<OperationEventArgs> OperationComplete;

        #endregion Events

        #region Properties

        public ICommand CancelCommand => this.cancelCommand ??
                                  (this.cancelCommand = new DelegateCommand(this.ExecuteCancelCommand));

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        #endregion Properties

        #region Methods

        protected virtual void CompleteOperation(bool isCanceled = false)
        {
            this.OperationComplete?.Invoke(this, new OperationEventArgs(this.Model, isCanceled));
        }

        private void ExecuteCancelCommand()
        {
            this.CompleteOperation(isCanceled: true);
        }

        #endregion Methods
    }
}
