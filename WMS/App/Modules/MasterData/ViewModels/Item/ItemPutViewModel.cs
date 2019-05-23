using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Core.Providers;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemPutViewModel : BaseDialogViewModel<ItemPut>
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private readonly IMaterialStatusProvider materialStatusProvider = ServiceLocator.Current.GetInstance<IMaterialStatusProvider>();

        private readonly IPackageTypeProvider packageTypeProvider = ServiceLocator.Current.GetInstance<IPackageTypeProvider>();

        private bool advancedPut;

        private ICommand runPutCommand;

        #endregion

        #region Constructors

        public ItemPutViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public bool AdvancedPut
        {
            get => this.advancedPut;
            set
            {
                this.SetProperty(ref this.advancedPut, value);
                if (!this.advancedPut)
                {
                    this.Model.Lot = null;
                    this.Model.RegistrationNumber = null;
                    this.Model.Sub1 = null;
                    this.Model.Sub2 = null;
                    this.Model.PackageTypeId = null;
                    this.Model.MaterialStatusId = null;
                }
            }
        }

        public ICommand RunPutCommand => this.runPutCommand ??
                    (this.runPutCommand = new DelegateCommand(
                    async () => await this.RunPutAsync(),
                    this.CanRunPut)
                .ObservesProperty(() => this.Model)
                .ObservesProperty(() => this.Model.Quantity));

        #endregion

        #region Methods

        protected override void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RunPutCommand)?.RaiseCanExecuteChanged();
        }

        protected override async void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.Model_PropertyChanged(sender, e);
            if (e == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(this.Model.AreaId):
                    this.Model.BayChoices = this.Model.AreaId.HasValue
                        ? await this.bayProvider.GetByAreaIdAsync(this.Model.AreaId.Value)
                        : null;
                    break;

                case nameof(this.Model.ItemDetails):
                    this.Model.AreaChoices = this.Model.ItemDetails != null
                        ? await this.areaProvider.GetByItemIdAsync(this.Model.ItemDetails.Id)
                        : null;
                    break;
            }
        }

        protected override async Task OnAppearAsync()
        {
            this.IsBusy = true;

            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync();

            this.IsBusy = false;
        }

        private async Task AddEnumerationsAsync(ItemPut itemPut)
        {
            if (itemPut != null)
            {
                itemPut.MaterialStatusChoices = await this.materialStatusProvider.GetAllAsync();
                itemPut.PackageTypeChoices = await this.packageTypeProvider.GetAllAsync();
            }
        }

        private bool CanRunPut()
        {
            return !this.IsBusy;
        }

        private void Initialize()
        {
            this.Model = new ItemPut();
        }

        private async Task LoadDataAsync()
        {
            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.Model.ItemDetails = await this.itemProvider.GetByIdAsync(modelId.Value).ConfigureAwait(true);
            await this.AddEnumerationsAsync(this.Model);
        }

        private async Task RunPutAsync()
        {
            if (!this.CheckValidModel())
            {
                return;
            }

            this.IsBusy = true;

            var result = await this.itemProvider.PutAsync(this.Model);

            this.IsBusy = false;

            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(
                    Common.Resources.MasterData.ItemPutCommenced,
                    StatusType.Success));

                this.CloseDialogCommand.Execute(null);
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(
                    result.Description,
                    StatusType.Error));
            }

            this.IsBusy = false;
        }

        #endregion
    }
}
