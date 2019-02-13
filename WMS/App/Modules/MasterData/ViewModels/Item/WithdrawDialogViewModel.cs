using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class WithdrawDialogViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private bool advancedWithdraw;

        private ICommand advancedWithdrawCommand;

        private bool isBusy;

        private ItemWithdraw itemWithdraw;

        private ICommand runWithdrawCommand;

        private ICommand simpleWithdrawCommand;

        private bool validationEnabled;

        private string validationError;

        #endregion

        #region Constructors

        public WithdrawDialogViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public bool AdvancedWithdraw
        {
            get => this.advancedWithdraw;
            set
            {
                this.SetProperty(ref this.advancedWithdraw, value);
                this.RaisePropertyChanged(nameof(this.SimpleWithdraw));
            }
        }

        public ICommand AdvancedWithdrawCommand => this.advancedWithdrawCommand ??
                                                   (this.advancedWithdrawCommand = new DelegateCommand(
                                                       this.ExecuteAdvancedWithdrawCommand));

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public ItemWithdraw ItemWithdraw
        {
            get => this.itemWithdraw;
            set
            {
                if (this.ItemWithdraw != null && value != this.ItemWithdraw)
                {
                    this.ItemWithdraw.PropertyChanged -= this.OnItemWithdrawPropertyChanged;
                }

                if (this.SetProperty(ref this.itemWithdraw, value))
                {
                    this.ItemWithdraw.PropertyChanged += this.OnItemWithdrawPropertyChanged;
                }
            }
        }

        public ICommand RunWithdrawCommand => this.runWithdrawCommand ??
                                              (this.runWithdrawCommand = new DelegateCommand(
                                                                     async () => await this.ExecuteRunWithdrawAsync(),
                                                                     this.CanExecuteRunWithdraw)
                                                  .ObservesProperty(() => this.ItemWithdraw)
                                                  .ObservesProperty(() => this.ItemWithdraw.Quantity));

        public bool SimpleWithdraw => !this.advancedWithdraw;

        public ICommand SimpleWithdrawCommand => this.simpleWithdrawCommand ??
                                                 (this.simpleWithdrawCommand = new DelegateCommand(
                                                     this.ExecuteSimpleWithdrawCommand));

        public string ValidationError
        {
            get => this.validationError;
            set => this.SetProperty(ref this.validationError, value);
        }

        #endregion

        #region Methods

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync();
            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.ItemWithdraw.ItemDetails = await this.itemProvider.GetByIdAsync(modelId.Value);
        }

        protected override void OnDispose()
        {
            this.ItemWithdraw.PropertyChanged -= this.OnItemWithdrawPropertyChanged;
            base.OnDispose();
        }

        private bool CanExecuteRunWithdraw()
        {
            return !this.validationEnabled || this.ExecuteValidation();
        }

        private void ExecuteAdvancedWithdrawCommand()
        {
            this.AdvancedWithdraw = true;
        }

        private async Task ExecuteRunWithdrawAsync()
        {
            this.validationEnabled = true;

            if (!this.ExecuteValidation())
            {
                ((DelegateCommand)this.RunWithdrawCommand)?.RaiseCanExecuteChanged();
                return;
            }

            this.IsBusy = true;

            var result = await this.itemProvider.WithdrawAsync(this.itemWithdraw);

            this.IsBusy = false;

            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemWithdrawCommenced, StatusType.Success));

                this.Disappear();
            }
            else
            {
                this.validationError = result.Description;
                this.EventService.Invoke(new StatusPubSubEvent(result.Description, StatusType.Error));
            }
        }

        private void ExecuteSimpleWithdrawCommand()
        {
            this.AdvancedWithdraw = false;
        }

        private bool ExecuteValidation()
        {
            var error = this.ItemWithdraw.Error;
            this.ValidationError = error;
            return this.ItemWithdraw != null && string.IsNullOrEmpty(error);
        }

        private void Initialize()
        {
            this.ItemWithdraw = new ItemWithdraw();
        }

        private void OnItemWithdrawPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.RunWithdrawCommand)?.RaiseCanExecuteChanged();

            switch (e.PropertyName)
            {
                case nameof(this.ItemWithdraw.AreaId):
                    this.ItemWithdraw.BayChoices = this.ItemWithdraw.AreaId.HasValue ?
                                                       this.bayProvider.GetByAreaId(this.ItemWithdraw.AreaId.Value) :
                                                       null;
                    break;

                case nameof(this.ItemWithdraw.ItemDetails):
                    this.ItemWithdraw.AreaChoices = this.ItemWithdraw.ItemDetails != null ?
                                                        this.areaProvider.GetByItemIdAvailability(this.ItemWithdraw.ItemDetails.Id) :
                                                        null;
                    break;
            }
        }

        #endregion
    }
}
