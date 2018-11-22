using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private BindableBase contentRegionCurrentViewModel;
        private bool machineModeSelectionBool = true;
        private int machineModeSelectionInt;
        private bool machineOnMarchSelectionBool = false;
        private int machineOnMarchSelectionInt;
        private BindableBase navigationRegionCurrentViewModel;

        #endregion Fields

        #region Properties

        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }
        public Boolean MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }

        public Int32 MachineModeSelectionInt
        {
            get => this.machineModeSelectionInt;
            set
            {
                this.SetProperty(ref this.machineModeSelectionInt, value);
                this.MachineModeSelectionBool = this.machineModeSelectionInt == 0 ? true : false;
            }
        }

        public Boolean MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }

        public Int32 MachineOnMarchSelectionInt
        {
            get => this.machineOnMarchSelectionInt;
            set
            {
                this.SetProperty(ref this.machineOnMarchSelectionInt, value);
                this.MachineOnMarchSelectionBool = this.machineOnMarchSelectionInt == 0 ? false : true;
            }
        }

        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        #endregion Properties
    }
}
