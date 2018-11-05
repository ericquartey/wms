using System;
using System.ComponentModel;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL;
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

        public bool SimpleWithdraw
        {
            get => !this.advancedWithdraw;
        }

        public ICommand SimpleWithdrawCommand => this.simpleWithdrawCommand ??
                                                 (this.simpleWithdrawCommand = new DelegateCommand(
                                                     this.ExecuteSimpleWithdrawCommand));

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.ItemWithdraw.Item = this.itemProvider.GetById(modelId.Value);
            this.ItemWithdraw.AreaChoices = this.areaProvider.GetByItemIdAvailability(modelId.Value);
            this.itemWithdraw.PropertyChanged += new PropertyChangedEventHandler(this.OnAreaIdChanged);
        }

        private bool CanExecuteRunWithdraw()
        {
            if (this.ItemWithdraw == null)
            {
                return false;
            }

            return this.ItemWithdraw.Quantity > 0 && this.ItemWithdraw.Quantity <= this.ItemWithdraw.TotalAvailable;
        }

        private void ExecuteAdvancedWithdrawCommand()
        {
            this.AdvancedWithdraw = true;
        }

        private void ExecuteRunWithdraw()
        {
            throw new NotImplementedException();
        }

        private void ExecuteSimpleWithdrawCommand()
        {
            this.AdvancedWithdraw = false;
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
            if (e.PropertyName == nameof(this.ItemWithdraw.Quantity))
            {
                ((DelegateCommand)this.RunWithdrawCommand)?.RaiseCanExecuteChanged();
            }
        }

        #endregion Methods
    }
}
