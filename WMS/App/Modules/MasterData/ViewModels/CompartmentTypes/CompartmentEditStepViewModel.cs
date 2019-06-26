using System;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentEditStepViewModel : StepViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private CompartmentEditViewModel compartmentAdd;

        private bool isBusy;

        private LoadingUnitDetails loadingUnitDetails;

        #endregion

        #region Constructors

        public CompartmentEditStepViewModel()
        {
            this.compartmentAdd = new CompartmentEditViewModel();
        }

        #endregion

        #region Properties

        public CompartmentEditViewModel CompartmentAdd
        {
            get => this.compartmentAdd;
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public LoadingUnitDetails LoadingUnitDetails
        {
            get => this.loadingUnitDetails;
        }

        #endregion

        #region Methods

        public override bool CanGoToNextView()
        {
            return false;
        }

        public override bool CanSave()
        {
            return true;
        }

        public override string GetError()
        {
            if (this.compartmentAdd == null ||
                this.compartmentAdd.Model == null)
            {
                return null;
            }

            return this.compartmentAdd.Model.Error;
        }

        public override async Task<bool> SaveAsync()
        {
            this.IsBusy = true;

            var result = await this.compartmentAdd.ExecuteCreateCommandAsync();

            this.IsBusy = false;

            return result;
        }

        protected override Task OnAppearAsync()
        {
            if (this.Data is Tuple<LoadingUnitDetails, ItemDetails> loadingUnitItem)
            {
                this.Title = string.Format(App.Resources.Title.CreateCompartmentForThisItem, loadingUnitItem.Item2.Code);

                this.loadingUnitDetails = loadingUnitItem.Item1;
                this.RaisePropertyChanged(nameof(this.LoadingUnitDetails));

                this.AddCompartmentAsync(loadingUnitItem.Item1, loadingUnitItem.Item2.Id).GetAwaiter();
            }

            return base.OnAppearAsync();
        }

        protected override void OnDispose()
        {
            if (this.compartmentAdd != null &&
                this.compartmentAdd.Model != null)
            {
                this.compartmentAdd.Model.PropertyChanged -= this.Model_PropertyChanged;
                this.loadingUnitDetails = null;
                this.compartmentAdd.Model = null;
                this.compartmentAdd = null;
            }

            this.Data = null;

            base.OnDispose();
        }

        private async Task AddCompartmentAsync(LoadingUnitDetails loadingUnitDetails, int itemId)
        {
            var result = await this.compartmentProvider.GetNewAsync();
            if (!result.Success)
            {
                return;
            }

            var model = result.Entity;
            model.LoadingUnitId = loadingUnitDetails.Id;
            model.LoadingUnit = loadingUnitDetails;

            this.compartmentAdd = new CompartmentEditViewModel
            {
                Model = model,
                Mode = CompartmentEditViewModel.AppearMode.Add,
                IsHeaderVisible = false,
                IsErrorsVisible = false
            };

            model.ItemId = itemId;
            model.PropertyChanged += this.Model_PropertyChanged;
            model.IsValidationEnabled = false;
            await this.compartmentAdd.InitializeDataAsync();
            this.RaisePropertyChanged(nameof(this.CompartmentAdd));
            this.NotifyToUpdate();
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BusinessObject.Error))
            {
                this.NotifyToUpdate();
            }
        }

        private void NotifyToUpdate()
        {
            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.UpdateError));
            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.UpdateCanSave));
        }

        #endregion
    }
}
