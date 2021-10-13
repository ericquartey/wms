using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class WaitingListEvadabilityOptionsViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IMachineItemListsWebService itemListsWebService;

        private bool isPartiallyExecute;

        private bool isPartiallyExecuteAndComplete;

        private bool isPartiallyExecuteAndWait;

        private DelegateCommand listExecuteCommand;

        private string waitingListCode;

        private string waitingListDescription;

        #endregion

        #region Constructors

        public WaitingListEvadabilityOptionsViewModel(
            IMachineItemListsWebService itemListsWebService)
            : base(PresentationMode.Operator)
        {
            this.itemListsWebService = itemListsWebService ?? throw new ArgumentNullException(nameof(itemListsWebService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ListSearch.ToString();

        public int AreaId { get; set; }

        public string AuthenticationUserName { get; set; }

        public int? BayId { get; set; }

        public bool IsPartiallyExecute
        {
            get => this.isPartiallyExecute;
            set
            {
                if (this.SetProperty(ref this.isPartiallyExecute, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsPartiallyExecuteAndComplete
        {
            get => this.isPartiallyExecuteAndComplete;
            set
            {
                if (this.SetProperty(ref this.isPartiallyExecuteAndComplete, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsPartiallyExecuteAndWait
        {
            get => this.isPartiallyExecuteAndWait;
            set
            {
                if (this.SetProperty(ref this.isPartiallyExecuteAndWait, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand ListExecuteCommand =>
            this.listExecuteCommand
            ??
            (this.listExecuteCommand = new DelegateCommand(
                async () => await this.ExecuteListAsync(),
                this.CanExecuteList));

        public int ListId { get; set; }

        public string WaitingListCode
        {
            get => this.waitingListCode;
            set => this.SetProperty(ref this.waitingListCode, value, this.RaiseCanExecuteChanged);
        }

        public string WaitingListDescription
        {
            get => this.waitingListDescription;
            set => this.SetProperty(ref this.waitingListDescription, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        public async Task ExecuteListAsync()
        {
            try
            {
                var type = this.GetEvadabilityType();
                _ = await this.itemListsWebService.ExecuteAsync(this.ListId, this.AreaId, type, this.BayId, this.AuthenticationUserName);

                this.ShowNotification(
                    string.Format(Resources.Localized.Get("OperatorApp.ExecutionOfListAccepted"), this.WaitingListCode),
                    Services.Models.NotificationSeverity.Success);

                this.NavigationService.GoBack();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(
                    Resources.Localized.Get("OperatorApp.CannotExecuteList"),
                    Services.Models.NotificationSeverity.Warning);
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            if (this.Data is WaitingListExecuteData item)
            {
                this.ListId = item.ListId;
                this.AreaId = item.AreaId;
                this.AuthenticationUserName = item.AuthenticationUserName;
                this.BayId = item.BayId;

                this.WaitingListCode = item.ListId.ToString();
                this.WaitingListDescription = item.ListDescription;

                this.IsPartiallyExecuteAndWait = true;
                this.IsPartiallyExecuteAndComplete = false;
                this.IsPartiallyExecute = false;

                this.RaiseCanExecuteChanged();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private bool CanExecuteList()
        {
            return true;
        }

        private ItemListEvadabilityType GetEvadabilityType()
        {
            var type = ItemListEvadabilityType.PartiallyExecuteAndWait;
            if (this.IsPartiallyExecuteAndWait)
            {
                type = ItemListEvadabilityType.PartiallyExecuteAndWait;
            }

            if (this.IsPartiallyExecuteAndComplete)
            {
                type = ItemListEvadabilityType.PartiallyExecuteAndComplete;
            }

            if (this.IsPartiallyExecute)
            {
                type = ItemListEvadabilityType.PartiallyExecute;
            }

            return type;
        }

        #endregion
    }
}
