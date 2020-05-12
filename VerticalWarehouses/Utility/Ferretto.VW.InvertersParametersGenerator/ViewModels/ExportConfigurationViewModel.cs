using System;
using System.Windows.Documents;
using System.Windows.Input;
using Ferretto.VW.InvertersParametersGenerator.Services;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    internal sealed class ExportConfigurationViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private readonly ConfigurationService configurationService;

        private readonly bool isSuccessful;

        private RelayCommand endCommand;

        private FlowDocument selectedStepLog = new FlowDocument();

        #endregion

        #region Constructors

        public ExportConfigurationViewModel(ConfigurationService installationService)
        {
            this.configurationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
        }

        #endregion

        #region Properties

        public ICommand EndCommand =>
            this.endCommand
            ??
            (this.endCommand = new RelayCommand(this.Export, this.CanClose));

        public bool IsSuccessful => this.isSuccessful;

        public FlowDocument SelectedStepLog
        {
            get => this.selectedStepLog;
            set => this.SetProperty(ref this.selectedStepLog, value);
        }

        #endregion

        #region Methods

        private bool CanClose()
        {
            return true;
        }

        private void Export()
        {
            this.configurationService.Export();
        }

        #endregion
    }
}
