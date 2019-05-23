using System;
using System.ComponentModel;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls.Interfaces;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public class BaseDialogViewModel<TModel> : BaseServiceNavigationViewModel, IExtensionDataEntityViewModel
        where TModel : class, ICloneable, IModel<int>, INotifyPropertyChanged, IDataErrorInfo, IValidationEnable
    {
        #region Fields

        private ICommand closeDialogCommand;

        private bool isBusy;

        private bool isModelValid;

        private TModel model;

        #endregion

        #region Constructors

        protected BaseDialogViewModel()
        {
            this.ChangeDetector.ModifiedChanged += this.ChangeDetector_ModifiedChanged;
        }

        #endregion

        #region Properties

        public ChangeDetector<TModel> ChangeDetector { get; } = new ChangeDetector<TModel>();

        public ICommand CloseDialogCommand => this.closeDialogCommand ??
            (this.closeDialogCommand = new DelegateCommand(
                this.ExecuteCloseDialogCommand));

        public ColorRequired ColorRequired => ColorRequired.CreateMode;

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

        public bool IsModelValid
        {
            get
            {
                var modelValid = this.Model == null || string.IsNullOrWhiteSpace(this.Model.Error);

                this.SetProperty(ref this.isModelValid, modelValid);
                return modelValid;
            }
        }

        public TModel Model
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
                    this.ChangeDetector.TakeSnapshot(this.model);

                    if (this.model != null)
                    {
                        this.model.PropertyChanged += this.Model_PropertyChanged;
                    }

                    this.LoadRelatedData();
                    this.EvaluateCanExecuteCommands();
                }
            }
        }

        #endregion

        #region Methods

        public virtual void LoadRelatedData()
        {
            // do nothing. The derived classes can customize the behaviour
        }

        protected virtual bool CheckValidModel()
        {
            if (this.Model == null)
            {
                return false;
            }

            this.Model.IsValidationEnabled = true;

            return this.IsModelValid
                && this.ChangeDetector.IsRequiredValid;
        }

        protected virtual void EvaluateCanExecuteCommands()
        {
        }

        protected void ExecuteCloseDialogCommand()
        {
            this.Disappear();
        }

        protected virtual void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (this.Model != null)
            {
                this.Model.PropertyChanged -= this.Model_PropertyChanged;
            }
        }

        protected void TakeModelSnapshot()
        {
            this.ChangeDetector.TakeSnapshot(this.Model);
        }

        private void ChangeDetector_ModifiedChanged(object sender, System.EventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        #endregion
    }
}
