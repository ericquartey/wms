using System;
using System.Threading.Tasks;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public class WmsCommand : DelegateCommand
    {
        #region Fields

        private readonly Action beforeExecuteActionFailed;

        private Func<Task<bool>> beforeExecuteAction;

        #endregion

        #region Constructors

        public WmsCommand(Action executeMethod)
                            : base(executeMethod)
        {
        }

        public WmsCommand(Action executeMethod, Func<bool> canExecuteMethod, Action beforeExecuteActionFailed)
            : base(executeMethod, canExecuteMethod)
        {
            this.beforeExecuteActionFailed = beforeExecuteActionFailed;
        }

        #endregion

        #region Methods

        public void BeforeExecute(Func<Task<bool>> action)
        {
            this.beforeExecuteAction = action;
        }

        protected override async void Execute(object parameter)
        {
            if (this.beforeExecuteAction != null)
            {
                var success = await this.beforeExecuteAction();
                if (success == false)
                {
                    this.beforeExecuteActionFailed?.Invoke();
                    return;
                }
            }

            base.Execute(parameter);
        }

        #endregion
    }
}
