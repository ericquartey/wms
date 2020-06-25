using System;
using System.Windows.Input;

namespace Ferretto.VW.Installer
{
    public class Command : ICommand
    {
        #region Fields

        private readonly Func<bool> canExecute;

        private readonly Action execute;

        private bool currentCanExecute;

        #endregion

        #region Constructors

        public Command(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged;

        #endregion

        #region Methods

        public bool CanExecute(object parameter)
        {
            if (this.canExecute is null)
            {
                return true;
            }

            var result = this.canExecute();
            if (this.currentCanExecute != result)
            {
                this.currentCanExecute = result;
                this.RaiseCanExecuteChanged();
            }

            return result;
        }

        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                this.execute();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, null);
        }

        #endregion
    }
}
