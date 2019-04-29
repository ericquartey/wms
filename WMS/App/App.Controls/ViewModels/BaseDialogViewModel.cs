using System;
using System.ComponentModel;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils;
using Prism.Commands;

namespace Ferretto.WMS.App.Controls
{
    public class BaseDialogViewModel<TModel> : BaseServiceNavigationViewModel
        where TModel : class, ICloneable, IModel<int>, INotifyPropertyChanged, IDataErrorInfo
    {
        #region Fields

        private readonly ChangeDetector<TModel> changeDetector = new ChangeDetector<TModel>();

        private bool canShowError;

        private ICommand closeDialogCommand;

        private bool isBusy;

        private bool isModelValid;

        private TModel model;

        #endregion

        #region Constructors

        protected BaseDialogViewModel()
        {
            this.changeDetector.ModifiedChanged += this.ChangeDetector_ModifiedChanged;
        }

        #endregion

        #region Properties

        public bool CanShowError
        {
            get => this.canShowError;
            set => this.SetProperty(ref this.canShowError, value);
        }

        public ChangeDetector<TModel> ChangeDetector { get => this.changeDetector; }

        public ICommand CloseDialogCommand => this.closeDialogCommand ??
                                    (this.closeDialogCommand = new DelegateCommand(
                        this.ExecuteCloseDialogCommand));

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

        #endregion

        #region Methods

        public virtual void LoadRelatedData()
        {
            // do nothing. The derived classes can customize the behaviour
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
