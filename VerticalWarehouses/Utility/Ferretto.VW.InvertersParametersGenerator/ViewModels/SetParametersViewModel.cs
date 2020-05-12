using System;
using System.Windows.Documents;
using System.Windows.Input;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.Services;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    internal sealed class SetParametersViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private readonly ConfigurationService configurationService;

        private readonly bool isSuccessful;

        private bool isFinished;

        private RelayCommand nextCommand;

        private FlowDocument selectedStepLog = new FlowDocument();

        #endregion

        #region Constructors

        public SetParametersViewModel(ConfigurationService installationService)
        {
            this.configurationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
        }

        #endregion

        #region Properties

        public bool IsFinished
        {
            get => this.isFinished;
            set => this.SetProperty(ref this.isFinished, value);
        }

        public bool IsSuccessful => this.isSuccessful;

        public ICommand NextCommand =>
                            this.nextCommand
            ??
            (this.nextCommand = new RelayCommand(this.Next, this.CanNext));

        public FlowDocument SelectedStepLog
        {
            get => this.selectedStepLog;
            set => this.SetProperty(ref this.selectedStepLog, value);
        }

        #endregion

        #region Methods

        private bool CanNext()
        {
            return true;
        }

        private void Next()
        {
            this.configurationService.SetWizard(WizardMode.ExportConfiguration);
        }

        #endregion
    }
}
