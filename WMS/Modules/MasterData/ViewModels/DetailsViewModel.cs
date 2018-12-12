using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public abstract class DetailsViewModel<T> : BaseServiceNavigationViewModel
        where T : BusinessObject
    {
        #region Fields

        private readonly ChangeDetector<T> changeDetector = new ChangeDetector<T>();

        private ICommand revertCommand;
        private ICommand saveCommand;

        #endregion Fields

        #region Constructors

        public DetailsViewModel()
        {
            this.changeDetector.ModifiedChanged += this.ChangeDetector_ModifiedChanged;
        }

        #endregion Constructors

        #region Properties

        public ICommand RevertCommand => this.revertCommand ??
                                         (this.revertCommand = new DelegateCommand(this.ExecuteRevertCommand, this.CanExecuteRevertCommand));

        public ICommand SaveCommand => this.saveCommand ??
                                       (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand, this.CanExecuteSaveCommand));

        #endregion Properties

        #region Methods

        protected virtual bool CanExecuteRevertCommand()
        {
            return this.changeDetector.IsModified == true;
        }

        protected virtual bool CanExecuteSaveCommand()
        {
            return this.changeDetector.IsModified == true;
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RevertCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
        }

        protected abstract void ExecuteRevertCommand();

        protected abstract void ExecuteSaveCommand();

        protected void TakeSnapshot(T model)
        {
            this.changeDetector.TakeSnapshot(model);
        }

        private void ChangeDetector_ModifiedChanged(System.Object sender, System.EventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        #endregion Methods
    }
}
