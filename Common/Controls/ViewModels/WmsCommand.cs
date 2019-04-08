using System;
using System.Threading.Tasks;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public class WmsCommand : DelegateCommand
    {
        #region Constructors

        public WmsCommand(Action executeMethod)
            : base(executeMethod)
        {
        }

        public WmsCommand(Action executeMethod, Func<bool> canExecuteMethod)
            : base(executeMethod, canExecuteMethod)
        {
        }

        #endregion

        #region Properties

        private Func<Task> ActionBefore { get; set; }

        #endregion

        #region Methods

        public void BeforeExecute(Func<Task> action)
        {
            this.ActionBefore = action;
        }

        protected override async void Execute(object parameter)
        {
            if (this.ActionBefore != null)
            {
                await this.ActionBefore();
            }

            base.Execute(parameter);
        }

        #endregion
    }
}
