using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.WMS.App.Controls
{
    public class WmsDelegateCommand : IWmsCommand
    {
        #region Fields

        private readonly Action beforeExecuteActionFailed;

        private readonly Func<bool> canExecute;

        private readonly Func<Task<bool>> completeExecute;

        private readonly Func<Task<bool>> execute;

        private Func<Task<bool>> afterExecuteAction;

        private Func<Task<bool>> beforeExecuteAction;

        #endregion

        #region Constructors

        public WmsDelegateCommand(Func<Task<bool>> execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public WmsDelegateCommand(Func<Task<bool>> execute, Func<bool> canExecute, Action beforeExecuteActionFailed)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.beforeExecuteActionFailed = beforeExecuteActionFailed;
        }

        public WmsDelegateCommand(Func<Task<bool>> execute, Func<bool> canExecute, Func<Task<bool>> completeExecute, Action beforeExecuteActionFailed)
        {
            this.execute = execute;
            this.canExecute = canExecute;
            this.completeExecute = completeExecute;
            this.beforeExecuteActionFailed = beforeExecuteActionFailed;
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Methods

        public void AfterExecute(Func<Task<bool>> action)
        {
            this.afterExecuteAction = action;
        }

        public void BeforeExecute(Func<Task<bool>> action)
        {
            this.beforeExecuteAction = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (this.beforeExecuteAction != null)
            {
                var resultBeforeExecute = await this.beforeExecuteAction();
                if (resultBeforeExecute == false)
                {
                    this.beforeExecuteActionFailed?.Invoke();
                    return;
                }
            }

            var resultExecute = await this.ExecuteAsync(parameter);

            if (resultExecute && this.afterExecuteAction != null)
            {
                var resultAfterExecute = await this.afterExecuteAction();
                if (resultAfterExecute && this.completeExecute != null)
                {
                    await this.completeExecute();
                }
            }
        }

        public async Task<bool> ExecuteAsync(object parameter = null)
        {
            var canExecuteResult = this.canExecute();
            return canExecuteResult ? await this.execute() : canExecuteResult;
        }

        public void RaiseCanExecuteChanged()
        {
            if (this.CanExecuteChanged == null)
            {
                this.CanExecuteChanged += this.WmsDelegateCommand_CanExecuteChanged;

                this.WmsDelegateCommand_CanExecuteChanged(null, EventArgs.Empty);
            }
        }

        private void WmsDelegateCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            this.canExecute();
        }

        #endregion
    }
}
