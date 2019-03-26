﻿using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public abstract class DetailsViewModel<T> : BaseServiceNavigationViewModel, IExtensionDataEntityViewModel
        where T : BusinessObject
    {
        #region Fields

        private readonly ChangeDetector<T> changeDetector = new ChangeDetector<T>();

        private ColorRequired colorRequired = ColorRequired.EditMode;

        private bool isBusy;

        private bool isModelValid;

        private T model;

        private ICommand refreshCommand;

        private ICommand revertCommand;

        private ICommand saveCommand;

        #endregion

        #region Constructors

        protected DetailsViewModel()
        {
            this.changeDetector.ModifiedChanged += this.ChangeDetector_ModifiedChanged;
        }

        #endregion

        #region Properties

        public ColorRequired ColorRequired
        {
            get => this.colorRequired;
            set => this.SetProperty(ref this.colorRequired, value);
        }

        public IDialogService DialogService { get; } = ServiceLocator.Current.GetInstance<IDialogService>();

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

        public bool IsModelIdValid => this.Model?.Id > 0;

        public bool IsModelValid
        {
            get
            {
                var temp = false;
                if (!this.changeDetector.IsModified || this.Model == null)
                {
                    temp = true;
                }
                else
                {
                    temp = string.IsNullOrWhiteSpace(this.Model.Error);
                }

                this.SetProperty(ref this.isModelValid, temp);
                return temp;
            }
        }

        public T Model
        {
            get => this.model;
            set
            {
                if (this.model != null)
                {
                    this.model.PropertyChanged -= this.Model_PropertyChanged;
                }

                if (this.SetProperty(ref this.model, value))
                {
                    this.changeDetector.TakeSnapshot(this.model);

                    if (this.model != null)
                    {
                        this.model.PropertyChanged += this.Model_PropertyChanged;
                    }

                    this.LoadRelatedData();
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

        public ICommand RefreshCommand => this.refreshCommand ??
                    (this.refreshCommand = new DelegateCommand(
                async () => await this.ExecuteRefreshCommandAsync(), this.CanExecuteRefreshCommand));

        public ICommand RevertCommand => this.revertCommand ??
            (this.revertCommand = new DelegateCommand(
                async () => await this.ExecuteRevertWithPromptAsync(),
                this.CanExecuteRevertCommand));

        public ICommand SaveCommand => this.saveCommand ??
            (this.saveCommand = new WmsCommand(
                async () => await this.ExecuteSaveCommandAsync(),
                this.CanExecuteSaveCommand));

        #endregion

        #region Methods

        public override bool CanDisappear()
        {
            if (this.changeDetector.IsModified)
            {
                var result = this.DialogService.ShowMessage(
                    DesktopApp.AreYouSureToLeaveThePage,
                    DesktopApp.ConfirmOperation,
                    DialogType.Exclamation,
                    DialogButtons.OKCancel);

                if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void LoadRelatedData()
        {
            // do nothing. The derived classes can customize the behaviour
        }

        public void ShowErrorDialog(string message)
        {
            this.DialogService.ShowMessage(
                message,
                DesktopApp.ConfirmOperation,
                DialogType.Warning,
                DialogButtons.OK);
        }

        protected virtual bool CanExecuteRevertCommand()
        {
            return this.changeDetector.IsModified == true
                && this.IsBusy == false;
        }

        protected virtual bool CanExecuteSaveCommand()
        {
            return this.Model != null
                && this.changeDetector.IsModified
                && this.IsModelValid
                && !this.IsBusy
                && this.changeDetector.IsRequiredValid;
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RevertCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.RefreshCommand)?.RaiseCanExecuteChanged();
        }

        protected abstract Task ExecuteRefreshCommandAsync();

        protected abstract Task ExecuteRevertCommandAsync();

        protected abstract Task ExecuteSaveCommandAsync();

        protected virtual void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (this.model != null)
            {
                this.model.PropertyChanged -= this.Model_PropertyChanged;
            }
        }

        protected void TakeModelSnapshot()
        {
            this.changeDetector.TakeSnapshot(this.model);
        }

        private bool CanExecuteRefreshCommand()
        {
            return !this.changeDetector.IsModified
                && !this.IsBusy;
        }

        private void ChangeDetector_ModifiedChanged(object sender, System.EventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        private async Task ExecuteRevertWithPromptAsync()
        {
            var result = this.DialogService.ShowMessage(
                DesktopApp.AreYouSureToRevertChanges,
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                await this.ExecuteRevertCommandAsync();
            }
        }

        #endregion
    }
}
