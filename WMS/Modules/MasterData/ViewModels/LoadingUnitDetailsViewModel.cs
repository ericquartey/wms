using System.Windows.Input;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitDetailsViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private object itemSelectionChangedSubscription;
        private LoadingUnitDetails loadingUnit;
        private ICommand saveCommand;

        #endregion Fields

        #region Constructors

        public LoadingUnitDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public LoadingUnitDetails LoadingUnit
        {
            get => this.loadingUnit;
            set => this.SetProperty(ref this.loadingUnit, value);
        }

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            this.LoadData(this.Data);
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ItemSelectionChangedEvent<Compartment>>(this.itemSelectionChangedSubscription);
            base.OnDispose();
        }

        private void ExecuteSaveCommand()
        {
            var rowSaved = this.loadingUnitProvider.Save(this.LoadingUnit);

            if (rowSaved != 0)
            {
                this.EventService.Invoke(new ItemChangedEvent<LoadingUnitDetails>(this.LoadingUnit));

                this.EventService.Invoke(new StatusEventArgs(Ferretto.Common.Resources.MasterData.LoadingUnitSavedSuccessfully));
            }
        }

        private void Initialize()
        {
            this.itemSelectionChangedSubscription = this.EventService.Subscribe<ItemSelectionChangedEvent<LoadingUnitDetails>>(
                    eventArgs => this.LoadData(eventArgs.ItemId), true);
        }

        private void LoadData(object itemId)
        {
            if (itemId == null)
            {
                this.LoadingUnit = null;
                return;
            }
            this.LoadingUnit = this.loadingUnitProvider.GetById((int)itemId);
        }

        #endregion Methods
    }
}
