using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.App.Services.Models
{
    public class MachineStatus : BindableBase
    {
        #region Fields

        private double? bayChainPosition;

        private string elevatorLogicalPosition;

        #endregion

        #region Properties

        public double? BayChainPosition
        {
            get => this.bayChainPosition;
            set => this.SetProperty(ref this.bayChainPosition, value);
        }

        public string ElevatorLogicalPosition
        {
            get => this.elevatorLogicalPosition;
            set => this.SetProperty(ref this.elevatorLogicalPosition, value);
        }

        #endregion
    }
}
