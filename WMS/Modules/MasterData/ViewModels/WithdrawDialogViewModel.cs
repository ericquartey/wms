using System;
using System.ComponentModel;
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
        private ItemWithdraw itemWithdraw;
        private ICommand runWithdrawCommand;
        private ICommand simpleWithdrawCommand;
        private bool validationEnabled = false;
        private string validationError;

        #endregion Fields

        #region Constructors

        public WithdrawDialogViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

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
                                              (this.runWithdrawCommand = new DelegateCommand(this.ExecuteRunWithdraw,
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

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.ItemWithdraw.ItemDetails = this.itemProvider.GetById(modelId.Value);
            this.ItemWithdraw.AreaChoices = this.areaProvider.GetByItemIdAvailability(modelId.Value);
            this.itemWithdraw.PropertyChanged += new PropertyChangedEventHandler(this.OnAreaIdChanged);
        }

        private bool CanExecuteRunWithdraw()
        {
            return !this.validationEnabled || this.ExecuteValidation();
        }

        private void ExecuteAdvancedWithdrawCommand()
        {
            this.AdvancedWithdraw = true;
        }

        private async void ExecuteRunWithdraw()
        {
            this.validationEnabled = true;

            if (!this.ExecuteValidation())
            {
                ((DelegateCommand)this.RunWithdrawCommand)?.RaiseCanExecuteChanged();
                return;
            }

            try
            {
                await this.itemProvider.WithdrawAsync(this.itemWithdraw);

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ItemWithdrawCommenced));
            }
            catch (Exception ex)
            {
                this.EventService.Invoke(new StatusEventArgs(ex.Message));
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
            return this.ItemWithdraw != null && String.IsNullOrEmpty(error);
        }

        private void Initialize()
        {
            this.ItemWithdraw = new ItemWithdraw();
        }

        private void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ItemWithdraw.AreaId))
            {
                this.ItemWithdraw.BayChoices = this.bayProvider.GetByAreaId(this.ItemWithdraw.AreaId);
            }
        }

        private void OnItemWithdrawPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.RunWithdrawCommand)?.RaiseCanExecuteChanged();
        }

        #endregion Methods
    }
}
