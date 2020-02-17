using System;
using System.Windows.Input;
using Ferretto.VW.Installer.Core;

namespace Ferretto.VW.Installer.ViewModels
{
    public class InstallBayViewModel : Core.BindableBase, IOperationResult
    {
        #region Fields

        private readonly InstallationService installationService;

        private bool canProcede;

        private bool isSuccessful;

        private RelayCommand nextCommand;

        private RelayCommand selectBayCommand;

        #endregion

        #region Constructors

        public InstallBayViewModel(InstallationService installationService)
        {
            this.installationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
        }

        #endregion

        #region Properties

        public bool IsSuccessful => throw new NotImplementedException();

        public ICommand NextCommand =>
                        this.nextCommand
                ??
                (this.nextCommand = new RelayCommand(this.Next, this.CanNext));

        public ICommand SelectBayCommand =>
                this.selectBayCommand
        ??
        (this.selectBayCommand = new RelayCommand(this.Next, this.CanNext));

        public virtual string Title { get; set; }

        #endregion

        #region Methods

        public void Save()
        {
            this.isSuccessful = true;
        }

        public void SelectBay()
        {
        }

        private bool CanNext()
        {
            return this.canProcede;
        }

        private bool CanOpenFile()
        {
            return true;
        }

        private void EvaluateCanNext()
        {
            this.canProcede = false;
            this.RaiseCanExecuteChanged();
        }

        private void Next()
        {
            try
            {
                this.installationService.SetOperation(OperationMode.ImstallType);
            }
            catch
            {
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.nextCommand.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
