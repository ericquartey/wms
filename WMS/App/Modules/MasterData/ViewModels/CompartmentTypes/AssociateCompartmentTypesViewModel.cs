using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class AssociateCompartmentTypesViewModel : StepViewModel
    {
        #region Fields

        private List<Enumeration> options;

        private Enumeration selectedOption;

        private string title;

        #endregion

        #region Constructors

        public AssociateCompartmentTypesViewModel()
        {
            this.InitializeData();
        }

        #endregion

        #region Properties

        public List<Enumeration> Options => this.options;

        public Enumeration SelectedOption
        {
            get => this.selectedOption;
            set
            {
                if (this.SetProperty(ref this.selectedOption, value))
                {
                    this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.Refresh));
                }
            }
        }

        public string Title => this.title;

        #endregion

        #region Methods

        public override bool CanGoToNextView()
        {
            if (this.selectedOption == null)
            {
                return false;
            }

            return true;
        }

        public override(string moduleName, string viewName, object data) GetNextView()
        {
            if (this.selectedOption == null)
            {
                return (null, null, null);
            }

            return (nameof(MasterData), Common.Utils.Modules.MasterData.ASSOCIATECOMPARTMENTTYPES, null);
        }

        protected override Task OnAppearAsync()
        {
            if (this.Data is ItemDetails itemDetails)
            {
                this.title = string.Format(Ferretto.Common.Resources.Title.AssociateCompartmentTypeToThisItem, itemDetails.Code);
                this.RaisePropertyChanged(nameof(this.Title));
            }

            return base.OnAppearAsync();
        }

        private void InitializeData()
        {
            this.options = new List<Enumeration>();
            this.options.Add(new Enumeration(1, Common.Resources.MasterData.AssociateACompartmentTypeToThisItem));
            this.options.Add(new Enumeration(2, Common.Resources.MasterData.CreateANewCompartment));
            this.RaisePropertyChanged(nameof(this.Options));
            this.SelectedOption = this.options.First();
        }

        #endregion
    }
}
