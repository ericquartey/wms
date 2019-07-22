using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Modules.MasterData
{
    public class AssociateCompartmentTypesStepViewModel : WmsWizardStepViewModel
    {
        #region Fields

        private List<Enumeration> options;

        private Enumeration selectedOption;

        #endregion

        #region Constructors

        public AssociateCompartmentTypesStepViewModel()
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

        public override bool CanSave()
        {
            if (this.selectedOption == null)
            {
                return false;
            }

            if (this.selectedOption.Id == this.options.Skip(1).First().Id)
            {
                return false;
            }

            return true;
        }

        public override (string moduleName, string viewName, object data) GetNextView()
        {
            if (this.selectedOption == null)
            {
                return (null, null, null);
            }

            if (this.selectedOption.Id == this.options.First().Id)
            {
                return (nameof(MasterData), Common.Utils.Modules.MasterData.CHOOSECOMPARTMENTTYPESSTEP, this.Data);
            }
            else
            {
                return (nameof(MasterData), Common.Utils.Modules.MasterData.CHOOSELOADINGUNITSTEP, this.Data);
            }
        }

        protected override Task OnAppearAsync()
        {
            if (this.Data is ItemDetails itemDetails)
            {
                this.Title = string.Format(App.Resources.Title.AssociateCompartmentTypeToThisItem, itemDetails.Code);
            }

            return base.OnAppearAsync();
        }

        private void InitializeData()
        {
            this.options = new List<Enumeration>();
            this.options.Add(new Enumeration(1, App.Resources.MasterData.AssociateACompartmentTypeToThisItem));
            this.options.Add(new Enumeration(2, App.Resources.MasterData.CreateANewCompartment));
            this.RaisePropertyChanged(nameof(this.Options));
            this.SelectedOption = this.options.First();
        }

        #endregion
    }
}
