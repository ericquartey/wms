using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class AddingItemDraperyToLoadingUnitViewModel : BaseOperatorViewModel
    {
        #region Fields

        private double draperyHeight;

        private string draperyId;

        private string draperyItemCode;

        private string draperyItemDescription;

        private DraperyItemInfo draperyItemInfo;

        private double draperyQuantity;

        #endregion

        #region Constructors

        public AddingItemDraperyToLoadingUnitViewModel()
            : base(PresentationMode.Operator)
        {
            this.Logger.Debug("Ctor AddingItemDraperyToLoadingUnitViewModel!");
        }

        #endregion

        #region Properties

        public double DraperyHeight
        {
            get => this.draperyHeight;
            set => this.SetProperty(ref this.draperyHeight, value, this.RaiseCanExecuteChanged);
        }

        public string DraperyId
        {
            get => this.draperyId;
            set => this.SetProperty(ref this.draperyId, value, this.RaiseCanExecuteChanged);
        }

        public string DraperyItemCode
        {
            get => this.draperyItemCode;
            set => this.SetProperty(ref this.draperyItemCode, value, this.RaiseCanExecuteChanged);
        }

        public string DraperyItemDescription
        {
            get => this.draperyItemDescription;
            set => this.SetProperty(ref this.draperyItemDescription, value, this.RaiseCanExecuteChanged);
        }

        public double DraperyQuantity
        {
            get => this.draperyQuantity;
            set => this.SetProperty(ref this.draperyQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool IsAddingAnErrorOperation { get; set; }

        public string MessageToShow { get; set; }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsAddingAnErrorOperation = false;
            this.MessageToShow = "Add a message";

            if (this.Data is DraperyItemInfo info)
            {
                this.draperyItemInfo = info;

                this.DraperyItemCode = this.draperyItemInfo.Item.Id.ToString();
                this.DraperyItemDescription = this.draperyItemInfo.Description;
                this.DraperyId = this.draperyItemInfo.Item.Code;
                this.DraperyQuantity = this.draperyItemInfo.Quantity;
                this.DraperyHeight = this.draperyItemInfo.Height;

                this.MessageToShow = this.draperyItemInfo.Note;
            }

            this.ClearNotifications();
            this.ShowNotification(this.MessageToShow, this.IsAddingAnErrorOperation ? Services.Models.NotificationSeverity.Error : Services.Models.NotificationSeverity.Success);
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
