using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.App.Services.Models
{
    public class MachineStatus : BindableBase, ICloneable
    {
        #region Fields

        private double? bayChainPosition;

        private string elevatorLogicalPosition;

        private string errorDescription;

        private bool isError;

        private bool isMoving;

        private bool isMovingElevator;

        private bool isMovingShutter;

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

        public string ErrorDescription
        {
            get => this.errorDescription;
            set => this.SetProperty(ref this.errorDescription, value);
        }

        public bool IsError
        {
            get => this.isError;
            set => this.SetProperty(ref this.isError, value);
        }

        public bool IsMoving
        {
            get => this.isMoving;
            set => this.SetProperty(ref this.isMoving, value);
        }

        public bool IsMovingElevator
        {
            get => this.isMovingElevator;
            set => this.SetProperty(ref this.isMovingElevator, value);
        }

        public bool IsMovingShutter
        {
            get => this.isMovingShutter;
            set => this.SetProperty(ref this.isMovingShutter, value);
        }

        #endregion

        #region Methods

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
